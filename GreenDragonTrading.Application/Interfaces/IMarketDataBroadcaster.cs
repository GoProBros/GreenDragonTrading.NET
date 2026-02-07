using GreenDragonTrading.Application.DTOs;

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
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="symbol">Symbol ticker</param>
        /// <param name="data">Data to broadcast</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task BroadcastMarketDataAsync<T>(string symbol, T data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Broadcast OHLCV candle update to specific ticker+timeframe group
        /// </summary>
        /// <param name="candle">Current candle data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task BroadcastOhlcvUpdateAsync(CurrentCandleDto candle, CancellationToken cancellationToken = default);

        /// <summary>
        /// Broadcast heatmap update to subscribed groups
        /// Triggered when market data changes
        /// </summary>
        /// <param name="heatmapData">Heatmap data to broadcast</param>
        /// <param name="exchange">Exchange filter (null = all)</param>
        /// <param name="sector">Sector filter (null = all)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task BroadcastHeatmapUpdateAsync(HeatmapDataDto heatmapData, string? exchange = null, string? sector = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Broadcast single heatmap item update in realtime (per symbol)
        /// Similar to OHLCV realtime broadcast pattern
        /// </summary>
        /// <param name="item">Single heatmap item</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task BroadcastHeatmapItemAsync(HeatmapItemDto item, CancellationToken cancellationToken = default);
    }
}
