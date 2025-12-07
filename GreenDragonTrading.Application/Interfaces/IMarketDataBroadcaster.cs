namespace GreenDragonTrading.Application.Interfaces
{
    /// <summary>
    /// Service for broadcasting market data to connected clients via SignalR
    /// </summary>
    public interface IMarketDataBroadcaster
    {
        /// <summary>
        /// Broadcast market data to specific symbol group
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="symbol"></param>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task BroadcastMarketDataAsync<T>(string symbol, T data, CancellationToken cancellationToken = default);
    }
}
