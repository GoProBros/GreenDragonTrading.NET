using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;

namespace GreenDragonTrading.Infrastructure.Services
{
    /// <summary>
    /// Service for real-time streaming market data from SSI using SignalR.
    /// Handles connection management, reconnection, and message broadcasting.
    /// </summary>
    public class SsiStreamingService : ISsiStreamingService, IDisposable
    {
        private readonly ILogger<SsiStreamingService> _logger;
        private readonly ISsiAuthService _authService;
        private readonly SsiApiOptionsV2 _options;

        private HubConnection? _hubConnection;
        private IHubProxy? _hubProxy;
        private Timer? _reconnectTimer;
        private string? _currentChannel;
        private bool _disposed;

        private const string HubName = "FcMarketDataV2Hub";
        private const string HubEndpoint = "v2.0/signalr";
        private const int ReconnectDelaySeconds = 3;

        /// <inheritdoc/>
        public event Func<string, Task>? OnBroadcastReceived;

        /// <inheritdoc/>
        public event Action<string>? OnErrorReceived;

        /// <inheritdoc/>
        public event Action<string, string>? OnStateChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="SsiStreamingService"/> class.
        /// </summary>
        /// <param name="logger">Logger instance for logging.</param>
        /// <param name="authService">Auth service for obtaining access tokens.</param>
        /// <param name="ssiApiOptionsV2">SSI API configuration options.</param>
        public SsiStreamingService(
            ILogger<SsiStreamingService> logger,
            ISsiAuthService authService,
            IOptions<SsiApiOptionsV2> ssiApiOptionsV2)
        {
            _logger = logger;
            _authService = authService;
            _options = ssiApiOptionsV2.Value;
        }

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            var accessToken = await _authService.GetAccessTokenAsync(cancellationToken);
            CreateHubConnection(accessToken);

            if (_hubConnection == null)
            {
                throw new InvalidOperationException("Failed to create hub connection.");
            }

            _logger.LogInformation("Starting SSI streaming connection...");
            await _hubConnection.Start(new WebSocketTransport());
            _logger.LogInformation("SSI streaming connection started successfully.");
        }

        /// <inheritdoc/>
        public void Stop()
        {
            if (_hubConnection == null) return;

            _logger.LogInformation("Stopping SSI streaming connection...");
            _hubConnection.Stop();
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

            _currentChannel = filterCondition;

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
            _reconnectTimer?.Dispose();
            _reconnectTimer = new Timer(
                async _ => await ReconnectAsync(),
                null,
                delaySeconds * 1000,
                Timeout.Infinite);

            _logger.LogInformation("Reconnection scheduled in {Seconds} seconds.", delaySeconds);
        }

        /// <summary>
        /// Attempts to reconnect to the SSI streaming hub.
        /// </summary>
        private async Task ReconnectAsync()
        {
            try
            {
                _logger.LogInformation("Attempting to reconnect to SSI streaming hub...");

                await StartAsync();

                if (!string.IsNullOrEmpty(_currentChannel))
                {
                    await SwitchChannelsAsync(_currentChannel);
                }

                _logger.LogInformation("Reconnection successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reconnection failed. Scheduling another attempt...");
                ScheduleReconnect();
            }
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
                _hubConnection?.Stop();
                _hubConnection?.Dispose();
            }

            _disposed = true;
        }
    }
}
