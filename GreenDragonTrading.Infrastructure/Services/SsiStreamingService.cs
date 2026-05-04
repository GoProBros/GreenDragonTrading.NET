using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;
using System.Collections.Concurrent;

namespace GreenDragonTrading.Infrastructure.Services
{
    /// <summary>
    /// Service for real-time streaming market data from SSI using SignalR.
    /// Handles connection management, reconnection, and message broadcasting.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="SsiStreamingService"/> class.
    /// </remarks>
    /// <param name="logger">Logger instance for logging.</param>
    /// <param name="authService">Auth service for obtaining access tokens.</param>
    /// <param name="ssiApiOptionsV2">SSI API configuration options.</param>
    public class SsiStreamingService(
        ILogger<SsiStreamingService> logger,
        ISsiAuthService authService,
        IOptions<SsiApiOptionsV2> ssiApiOptionsV2) : ISsiStreamingService, IDisposable
    {
        private readonly ILogger<SsiStreamingService> _logger = logger;
        private readonly ISsiAuthService _authService = authService;
        private readonly SsiApiOptionsV2 _options = ssiApiOptionsV2.Value;

        private HubConnection? _hubConnection;
        private IHubProxy? _hubProxy;
        private Timer? _reconnectTimer;
        private Timer? _proactiveReconnectTimer;
        private Timer? _connectionWatchdogTimer;
        private readonly SemaphoreSlim _reconnectSemaphore = new(1, 1);
        private readonly ConcurrentDictionary<string, byte> _subscribedChannels = new(StringComparer.Ordinal);
        private long _lastBroadcastTicksUtc = DateTime.UtcNow.Ticks;
        private DateTime _connectAttemptStartedUtc = DateTime.UtcNow;
        private int _reconnectAttempt;
        private bool _disposed;

        private const string HubName = "FcMarketDataV2Hub";
        private const string HubEndpoint = "v2.0/signalr";
        private const int ReconnectDelaySeconds = 3;
        private const int MaxReconnectDelaySeconds = 120;
        private const int MaxReconnectJitterMilliseconds = 1000;
        private const int WatchdogIntervalSeconds = 15;
        private const int MaxSilentSecondsBeforeReconnect = 45;
        private static readonly TimeZoneInfo VietnamTimeZone = ResolveVietnamTimeZone();

        /// <inheritdoc/>
        public event Func<string, Task>? OnBroadcastReceived;

        /// <inheritdoc/>
        public event Action<string>? OnErrorReceived;

        /// <inheritdoc/>
        public event Action<string, string>? OnStateChanged;

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            _connectAttemptStartedUtc = DateTime.UtcNow;
            var accessToken = await _authService.GetAccessTokenAsync(cancellationToken);
            CreateHubConnection(accessToken);

            if (_hubConnection == null)
            {
                throw new InvalidOperationException("Failed to create hub connection.");
            }

            _logger.LogInformation(
                "SSI connect starting. Url={Url}, ReconnectAttempt={ReconnectAttempt}",
                _hubConnection.Url,
                Volatile.Read(ref _reconnectAttempt));

            var transportConnectStartUtc = DateTime.UtcNow;

            try
            {
                // Prefer WebSocket for low-latency realtime streaming.
                await _hubConnection.Start(new WebSocketTransport());
                _logger.LogInformation(
                    "SSI connect succeeded via WebSocket. DurationMs={DurationMs:F0}",
                    (DateTime.UtcNow - transportConnectStartUtc).TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "SSI WebSocket connect failed. DurationMs={DurationMs:F0}. Falling back to negotiated transport.",
                    (DateTime.UtcNow - transportConnectStartUtc).TotalMilliseconds);

                var fallbackStartUtc = DateTime.UtcNow;
                try
                {
                    await _hubConnection.Start();
                    _logger.LogInformation(
                        "SSI connect succeeded via fallback transport. DurationMs={DurationMs:F0}",
                        (DateTime.UtcNow - fallbackStartUtc).TotalMilliseconds);
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(
                        fallbackEx,
                        "SSI fallback connect failed. DurationMs={DurationMs:F0}",
                        (DateTime.UtcNow - fallbackStartUtc).TotalMilliseconds);
                    throw;
                }
            }

            Interlocked.Exchange(ref _lastBroadcastTicksUtc, DateTime.UtcNow.Ticks);
            StartConnectionWatchdog();
            ScheduleDailyProactiveReconnect();
        }

        /// <inheritdoc/>
        public void Stop()
        {
            if (_hubConnection == null)
            {
                return;
            }

            _reconnectTimer?.Dispose();
            _reconnectTimer = null;
            _proactiveReconnectTimer?.Dispose();
            _proactiveReconnectTimer = null;
            _connectionWatchdogTimer?.Dispose();
            _connectionWatchdogTimer = null;
            Interlocked.Exchange(ref _reconnectAttempt, 0);

            _logger.LogInformation(
                "Stopping SSI streaming connection. CurrentState={State}, ChannelCount={ChannelCount}",
                _hubConnection.State,
                _subscribedChannels.Count);
            CleanupHubConnection();
            _logger.LogInformation("SSI streaming connection stopped.");
        }

        /// <inheritdoc/>
        public async Task SwitchChannelsAsync(string filterCondition)
        {
            if (string.IsNullOrEmpty(filterCondition))
            {
                _logger.LogWarning("SwitchChannelsAsync called with empty filter condition.");
                return;
            }

            _subscribedChannels.TryAdd(filterCondition, 0);

            if (_hubProxy != null)
            {
                _logger.LogInformation("Switching to channel: {Channel}", filterCondition);
                await _hubProxy.Invoke("SwitchChannels", filterCondition);
            }
            else
            {
                _logger.LogWarning("Cannot switch channels: HubProxy is not initialized.");
            }
        }

        /// <summary>
        /// Creates and configures the SignalR hub connection.
        /// </summary>
        /// <param name="accessToken">The access token for authentication.</param>
        private void CreateHubConnection(string accessToken)
        {
            CleanupHubConnection();

            var url = _options.StreamURL.TrimEnd('/') + "/" + HubEndpoint;
            _logger.LogInformation("Creating SSI hub connection. Url={Url}", url);

            _hubConnection = new HubConnection(url);
            _hubConnection.Headers.Add("Authorization", $"Bearer {accessToken}");

            _hubProxy = _hubConnection.CreateHubProxy(HubName);

            // Register event handlers
            _hubProxy.On<string>("Broadcast", HandleBroadcast);
            _hubProxy.On<string>("Error", HandleError);
            _hubConnection.StateChanged += HandleStateChanged;
        }

        /// <summary>
        /// Stops and disposes the current hub connection and detaches handlers.
        /// </summary>
        private void CleanupHubConnection()
        {
            if (_hubConnection == null)
            {
                _hubProxy = null;
                return;
            }

            try
            {
                _hubConnection.StateChanged -= HandleStateChanged;
                _hubConnection.Stop();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Ignoring error while stopping existing hub connection.");
            }
            finally
            {
                _hubConnection.Dispose();
                _hubConnection = null;
                _hubProxy = null;
            }
        }

        /// <summary>
        /// Handles broadcast messages from the hub.
        /// </summary>
        /// <param name="data">The broadcast data received.</param>
        private async void HandleBroadcast(string data)
        {
            try
            {
                Interlocked.Exchange(ref _lastBroadcastTicksUtc, DateTime.UtcNow.Ticks);
                _logger.LogDebug("Broadcast received: {Data}", data);
                if (OnBroadcastReceived != null)
                {
                    await OnBroadcastReceived.Invoke(data);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing broadcast event.");
            }
        }

        /// <summary>
        /// Handles error messages from the hub.
        /// </summary>
        /// <param name="message">The error message received.</param>
        private void HandleError(string message)
        {
            try
            {
                _logger.LogWarning(
                    "SSI hub error received. State={State}, Message={Message}",
                    _hubConnection?.State,
                    message);
                OnErrorReceived?.Invoke(message);

                if (ShouldReconnectFromError(message))
                {
                    _logger.LogWarning(
                        "Detected recoverable SSI streaming error (504/timeout). Scheduling reconnect.");
                    ScheduleReconnect(1, "hub-error-timeout");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing error event.");
            }
        }

        /// <summary>
        /// Determines whether the SSI error payload indicates a stale/broken connection
        /// that should trigger an immediate reconnect attempt.
        /// </summary>
        private static bool ShouldReconnectFromError(string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            return message.Contains("504", StringComparison.OrdinalIgnoreCase)
                || message.Contains("gateway timeout", StringComparison.OrdinalIgnoreCase)
                || message.Contains("timeout", StringComparison.OrdinalIgnoreCase)
                || message.Contains("timed out", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Handles connection state changes.
        /// </summary>
        /// <param name="stateChange">The state change information.</param>
        private void HandleStateChanged(StateChange stateChange)
        {
            try
            {
                _logger.LogInformation(
                    "SSI hub state changed: {OldState}->{NewState}, ReconnectAttempt={ReconnectAttempt}",
                    stateChange.OldState,
                    stateChange.NewState,
                    Volatile.Read(ref _reconnectAttempt));

                OnStateChanged?.Invoke(stateChange.OldState.ToString(), stateChange.NewState.ToString());

                switch (stateChange.NewState)
                {
                    case ConnectionState.Connected:
                        Interlocked.Exchange(ref _lastBroadcastTicksUtc, DateTime.UtcNow.Ticks);
                        _logger.LogInformation(
                            "SSI hub connected. ConnectDurationMs={DurationMs:F0}",
                            (DateTime.UtcNow - _connectAttemptStartedUtc).TotalMilliseconds);
                        break;
                    case ConnectionState.Reconnecting:
                        HandleReconnecting();
                        break;
                    case ConnectionState.Disconnected:
                        ScheduleReconnect(reason: $"state:{stateChange.OldState}->{stateChange.NewState}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing state change event.");
            }
        }

        /// <summary>
        /// Handles the reconnecting state by refreshing the access token.
        /// </summary>
        private void HandleReconnecting()
        {
            try
            {
                if (_hubConnection == null) return;

                var newToken = _authService.GetAccessTokenAsync().GetAwaiter().GetResult();
                _hubConnection.Headers.Remove("Authorization");
                _hubConnection.Headers.Add("Authorization", $"Bearer {newToken}");

                _logger.LogInformation("Access token refreshed during reconnection.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token during reconnection.");
            }
        }

        /// <summary>
        /// Schedules a reconnection attempt after a delay.
        /// </summary>
        /// <param name="delaySeconds">The delay in seconds before attempting to reconnect.</param>
        private void ScheduleReconnect(int delaySeconds = ReconnectDelaySeconds, string reason = "unspecified")
        {
            if (_disposed)
            {
                return;
            }

            var attempt = Interlocked.Increment(ref _reconnectAttempt);
            var exponentialDelaySeconds = (int)Math.Min(
                MaxReconnectDelaySeconds,
                ReconnectDelaySeconds * Math.Pow(2, Math.Min(attempt - 1, 10)));

            var forcedDelaySeconds = delaySeconds > 0 ? Math.Min(delaySeconds, MaxReconnectDelaySeconds) : exponentialDelaySeconds;
            var jitterMilliseconds = Random.Shared.Next(0, MaxReconnectJitterMilliseconds + 1);
            var dueTime = TimeSpan.FromSeconds(forcedDelaySeconds) + TimeSpan.FromMilliseconds(jitterMilliseconds);

            _reconnectTimer?.Dispose();
            _reconnectTimer = new Timer(
                _ => _ = ReconnectAsync(),
                null,
                dueTime,
                Timeout.InfiniteTimeSpan);

            _logger.LogWarning(
                "Reconnect scheduled. Reason={Reason}, DelaySeconds={DelaySeconds}, JitterMs={JitterMs}, Attempt={Attempt}",
                reason,
                forcedDelaySeconds,
                jitterMilliseconds,
                attempt);
        }

        /// <summary>
        /// Attempts to reconnect to the SSI streaming hub.
        /// </summary>
        private async Task ReconnectAsync()
        {
            if (_disposed)
            {
                return;
            }

            var reconnectStartUtc = DateTime.UtcNow;

            if (!await _reconnectSemaphore.WaitAsync(0))
            {
                _logger.LogDebug(
                    "Reconnect already in progress. Skipping duplicate attempt.");
                return;
            }

            try
            {
                _logger.LogInformation(
                    "Reconnect attempt started. Attempt={Attempt}",
                    Volatile.Read(ref _reconnectAttempt));

                await StartAsync();

                await ResubscribeAllChannelsAsync();

                _reconnectTimer?.Dispose();
                _reconnectTimer = null;
                Interlocked.Exchange(ref _reconnectAttempt, 0);

                _logger.LogInformation(
                    "Reconnect successful. DurationMs={DurationMs:F0}, ReSubscribedChannels={ChannelCount}",
                    (DateTime.UtcNow - reconnectStartUtc).TotalMilliseconds,
                    _subscribedChannels.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Reconnect failed. DurationMs={DurationMs:F0}. Scheduling retry.",
                    (DateTime.UtcNow - reconnectStartUtc).TotalMilliseconds);
                ScheduleReconnect(reason: "reconnect-failed");
            }
            finally
            {
                _reconnectSemaphore.Release();
            }
        }

        /// <summary>
        /// Starts a lightweight watchdog that detects silent/stale connections
        /// (Connected state but no broadcasts received) and triggers reconnect.
        /// </summary>
        private void StartConnectionWatchdog()
        {
            if (_disposed)
            {
                return;
            }

            _connectionWatchdogTimer?.Dispose();
            _connectionWatchdogTimer = new Timer(
                _ => _ = CheckConnectionHealthAsync(),
                null,
                TimeSpan.FromSeconds(WatchdogIntervalSeconds),
                TimeSpan.FromSeconds(WatchdogIntervalSeconds));

            _logger.LogInformation(
                "Started SSI connection watchdog. IntervalSeconds={IntervalSeconds}, MaxSilentSeconds={MaxSilentSeconds}",
                WatchdogIntervalSeconds,
                MaxSilentSecondsBeforeReconnect);
        }

        /// <summary>
        /// Verifies stream liveness and schedules reconnect when stream is stale.
        /// </summary>
        private Task CheckConnectionHealthAsync()
        {
            try
            {
                if (_disposed || _hubConnection == null)
                {
                    return Task.CompletedTask;
                }

                if (_hubConnection.State != ConnectionState.Connected || _subscribedChannels.IsEmpty)
                {
                    return Task.CompletedTask;
                }

                if (!IsStreamingExpectedNow(DateTimeOffset.UtcNow))
                {
                    return Task.CompletedTask;
                }

                var lastTicks = Interlocked.Read(ref _lastBroadcastTicksUtc);
                var lastBroadcastUtc = new DateTime(lastTicks, DateTimeKind.Utc);
                var silence = DateTime.UtcNow - lastBroadcastUtc;

                if (silence.TotalSeconds < MaxSilentSecondsBeforeReconnect)
                {
                    return Task.CompletedTask;
                }

                _logger.LogWarning(
                    "SSI stream stale. State={State}, SilenceSeconds={SilenceSeconds:F0}. Scheduling reconnect.",
                    _hubConnection.State,
                    silence.TotalSeconds);

                // Rate-limit repeated watchdog-triggered reconnect logs until next message arrives.
                Interlocked.Exchange(ref _lastBroadcastTicksUtc, DateTime.UtcNow.Ticks);
                ScheduleReconnect(1, "watchdog-silent-stream");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Connection watchdog check failed.");
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns true during VN trading window when realtime stream is expected.
        /// </summary>
        private static bool IsStreamingExpectedNow(DateTimeOffset utcNow)
        {
            var vnNow = TimeZoneInfo.ConvertTime(utcNow, VietnamTimeZone);
            if (vnNow.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                return false;
            }

            var t = vnNow.TimeOfDay;
            return t >= new TimeSpan(8, 45, 0) && t <= new TimeSpan(15, 30, 0);
        }

        /// <summary>
        /// Schedules proactive reconnect at 07:00 GMT+7 every day.
        /// </summary>
        private void ScheduleDailyProactiveReconnect()
        {
            if (_disposed)
            {
                return;
            }

            var nowUtc = DateTimeOffset.UtcNow;
            var nowVn = TimeZoneInfo.ConvertTime(nowUtc, VietnamTimeZone);
            var nextRunVn = new DateTimeOffset(
                nowVn.Year,
                nowVn.Month,
                nowVn.Day,
                7,
                0,
                0,
                nowVn.Offset);

            if (nowVn >= nextRunVn)
            {
                nextRunVn = nextRunVn.AddDays(1);
            }

            var dueTime = nextRunVn.ToUniversalTime() - nowUtc;
            if (dueTime < TimeSpan.Zero)
            {
                dueTime = TimeSpan.Zero;
            }

            _proactiveReconnectTimer?.Dispose();
            _proactiveReconnectTimer = new Timer(
                _ => _ = HandleProactiveReconnectTimerAsync(),
                null,
                dueTime,
                Timeout.InfiniteTimeSpan);

            _logger.LogInformation(
                "Scheduled proactive reconnect at {NextRunVn} (GMT+7), in {DueMinutes:F1} minutes.",
                nextRunVn,
                dueTime.TotalMinutes);
        }

        /// <summary>
        /// Handles the proactive reconnect timer tick and schedules the next run.
        /// </summary>
        private async Task HandleProactiveReconnectTimerAsync()
        {
            try
            {
                if (_disposed)
                {
                    return;
                }

                _logger.LogInformation("Running proactive reconnect at scheduled 07:00 GMT+7 window.");
                await ReconnectAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Proactive reconnect execution failed.");
            }
            finally
            {
                ScheduleDailyProactiveReconnect();
            }
        }

        /// <summary>
        /// Resolves Vietnam time zone across Windows and Linux runtimes.
        /// </summary>
        private static TimeZoneInfo ResolveVietnamTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
        }

        /// <summary>
        /// Re-subscribes all channels that were requested before disconnection.
        /// </summary>
        private async Task ResubscribeAllChannelsAsync()
        {
            if (_hubProxy == null)
            {
                _logger.LogWarning("Cannot re-subscribe channels: HubProxy is not initialized.");
                return;
            }

            var channels = _subscribedChannels.Keys.ToArray();
            if (channels.Length == 0)
            {
                _logger.LogInformation("No channels to re-subscribe after reconnect.");
                return;
            }

            foreach (var channel in channels)
            {
                try
                {
                    // _logger.LogInformation("Re-subscribing channel after reconnect: {Channel}", channel);
                    await _hubProxy.Invoke("SwitchChannels", channel);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to re-subscribe channel {Channel}.", channel);
                }
            }

            _logger.LogInformation("Re-subscribed {Count} channels after reconnect.", channels.Length);
        }

        /// <summary>
        /// Throws if this service has already been disposed.
        /// </summary>
        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }

        /// <summary>
        /// Disposes the streaming service resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the streaming service resources.
        /// </summary>
        /// <param name="disposing">True if disposing managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _reconnectTimer?.Dispose();
                _proactiveReconnectTimer?.Dispose();
                _connectionWatchdogTimer?.Dispose();
                _reconnectSemaphore.Dispose();
                CleanupHubConnection();
            }

            _disposed = true;
        }
    }
}
