using GreenDragonTrading.Application.DTOs;

namespace GreenDragonTrading.Application.Interfaces
{
    /// <summary>
    /// Service for aggregating real-time tick data into OHLCV candles
    /// </summary>
    public interface IOhlcvAggregationService
    {
        /// <summary>
        /// Process a single tick and update all relevant candles
        /// </summary>
        /// <param name="tick">Tick data from market stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ProcessTickAsync(TickData tick, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get current (in-progress) candle for a specific ticker and timeframe
        /// </summary>
        /// <param name="ticker">Stock ticker symbol</param>
        /// <param name="timeframe">Timeframe (M1, M5, M15, H1, H4, D1)</param>
        /// <returns>Current candle DTO or null if not found</returns>
        CurrentCandleDto? GetCurrentCandle(string ticker, string timeframe);

        /// <summary>
        /// Get all current candles for a ticker across all timeframes
        /// </summary>
        /// <param name="ticker">Stock ticker symbol</param>
        /// <returns>Dictionary of timeframe to current candle</returns>
        Dictionary<string, CurrentCandleDto> GetAllCurrentCandles(string ticker);

        /// <summary>
        /// Flush all in-progress candles to database (called on shutdown)
        /// </summary>
        Task FlushAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Initialize service and recover any incomplete candles from database
        /// </summary>
        Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Check and close any candles that have exceeded their time period
        /// Called by periodic timer to handle gaps in data
        /// </summary>
        Task CheckAndCloseExpiredCandlesAsync(DateTime currentTime, CancellationToken cancellationToken = default);
    }
}
