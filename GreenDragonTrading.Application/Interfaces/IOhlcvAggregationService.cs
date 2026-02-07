using GreenDragonTrading.Application.DTOs;

namespace GreenDragonTrading.Application.Interfaces
{
    /// <summary>
    /// Service for aggregating real-time tick data into OHLCV candles
    /// NOTE: This service is legacy and may not be actively used after OHLCV refactoring
    /// </summary>
    public interface IOhlcvAggregationService
    {
        /// <summary>
        /// Initialize the service and recover incomplete candles from database
        /// </summary>
        Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Process a single tick and update all timeframe candles
        /// </summary>
        Task ProcessTickAsync(TickData tick, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get current candle for specific ticker and timeframe
        /// </summary>
        CurrentCandleDto? GetCurrentCandle(string ticker, string timeframe);

        /// <summary>
        /// Get all current candles for a ticker (all timeframes)
        /// </summary>
        Dictionary<string, CurrentCandleDto> GetAllCurrentCandles(string ticker);

        /// <summary>
        /// Force close and save all incomplete candles (used for graceful shutdown)
        /// </summary>
        Task FlushAllCandlesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Check and close expired candles periodically
        /// </summary>
        Task CheckExpiredCandlesAsync(CancellationToken cancellationToken = default);
    }
}
