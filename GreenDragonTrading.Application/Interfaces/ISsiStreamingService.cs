namespace GreenDragonTrading.Application.Interfaces
{
    /// <summary>
    /// Interface for SSI real-time streaming service using SignalR.
    /// Provides methods to connect, subscribe to channels, and receive market data.
    /// </summary>
    public interface ISsiStreamingService
    {
        /// <summary>
        /// Starts the SignalR connection to SSI streaming hub.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stops the SignalR connection to SSI streaming hub.
        /// </summary>
        void Stop();

        /// <summary>
        /// Switches or subscribes to a specific channel with filter conditions.
        /// </summary>
        /// <param name="filterCondition">The filter condition for channel subscription (e.g., symbol list).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SwitchChannelsAsync(string filterCondition);

        /// <summary>
        /// Event triggered when broadcast data is received from SSI.
        /// </summary>
        event Action<string>? OnBroadcastReceived;

        /// <summary>
        /// Event triggered when an error message is received from SSI.
        /// </summary>
        event Action<string>? OnErrorReceived;

        /// <summary>
        /// Event triggered when connection state changes.
        /// </summary>
        event Action<string, string>? OnStateChanged;
    }
}
