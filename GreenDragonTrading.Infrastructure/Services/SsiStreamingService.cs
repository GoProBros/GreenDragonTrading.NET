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
        private readonly SemaphoreSlim _reconnectSemaphore = new(1, 1);
        private readonly ConcurrentDictionary<string, byte> _subscribedChannels = new(StringComparer.Ordinal);
        private int _reconnectAttempt;
        private bool _disposed;

        private const string HubName = "FcMarketDataV2Hub";
        private const string HubEndpoint = "v2.0/signalr";
        private const int ReconnectDelaySeconds = 3;
        private const int MaxReconnectDelaySeconds = 120;
        private const int MaxReconnectJitterMilliseconds = 1000;
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

            var accessToken = await _authService.GetAccessTokenAsync(cancellationToken);
            CreateHubConnection(accessToken);

            if (_hubConnection == null)
            {
                throw new InvalidOperationException("Failed to create hub connection.");
            }

            _logger.LogInformation("Starting SSI streaming connection...");

            try
            {
                // Prefer WebSocket for low-latency realtime streaming.
                await _hubConnection.Start(new WebSocketTransport());
                _logger.LogInformation("SSI streaming connection started successfully via WebSocket transport.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "WebSocket transport start failed. Falling back to default SignalR transport negotiation.");
                await _hubConnection.Start();
                _logger.LogInformation("SSI streaming connection started successfully via fallback transport.");
            }

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
            Interlocked.Exchange(ref _reconnectAttempt, 0);

            _logger.LogInformation("Stopping SSI streaming connection...");
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

            _logger.LogInformation("Creating hub connection to: {Url}", url);

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
                _logger.LogWarning("Error received from SSI hub: {Message}", message);
                OnErrorReceived?.Invoke(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing error event.");
            }
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
                    "SSI hub connection state changed from {OldState} to {NewState}",
                    stateChange.OldState,
                    stateChange.NewState);

                OnStateChanged?.Invoke(stateChange.OldState.ToString(), stateChange.NewState.ToString());

                switch (stateChange.NewState)
                {
                    case ConnectionState.Reconnecting:
                        HandleReconnecting();
                        break;
                    case ConnectionState.Disconnected:
                        ScheduleReconnect();
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
        private void ScheduleReconnect(int delaySeconds = ReconnectDelaySeconds)
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
                "Reconnection scheduled in {DelaySeconds}s (+{JitterMs}ms jitter). Attempt #{Attempt}.",
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

            if (!await _reconnectSemaphore.WaitAsync(0))
            {
                _logger.LogDebug("Reconnect is already in progress. Skipping duplicate attempt.");
                return;
            }

            try
            {
                _logger.LogInformation("Attempting to reconnect to SSI streaming hub...");

                await StartAsync();

                await ResubscribeAllChannelsAsync();

                _reconnectTimer?.Dispose();
                _reconnectTimer = null;
                Interlocked.Exchange(ref _reconnectAttempt, 0);

                _logger.LogInformation("Reconnection successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reconnection failed. Scheduling another attempt...");
                ScheduleReconnect();
            }
            finally
            {
                _reconnectSemaphore.Release();
            }
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
                    _logger.LogInformation("Re-subscribing channel after reconnect: {Channel}", channel);
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
                _reconnectSemaphore.Dispose();
                CleanupHubConnection();
            }

            _disposed = true;
        }
    }
}
