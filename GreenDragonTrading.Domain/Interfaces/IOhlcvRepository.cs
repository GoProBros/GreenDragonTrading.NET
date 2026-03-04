using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces
{
    /// <summary>
    /// Repository interface cho OHLCV data access
    /// </summary>
    public interface IOhlcvRepository
    {
        /// <summary>
        /// Thêm một nến OHLCV
        /// </summary>
        Task AddAsync(Ohlcv ohlcv, CancellationToken cancellationToken = default);

        /// <summary>
        /// Thêm nhiều nến OHLCV (bulk insert)
        /// </summary>
        Task AddRangeAsync(IEnumerable<Ohlcv> ohlcvList, CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk upsert với ON CONFLICT DO NOTHING - Insert nhiều records, bỏ qua duplicates
        /// Returns số records đã insert thành công (không tính duplicates)
        /// </summary>
        Task<int> BulkUpsertAsync(IEnumerable<Ohlcv> ohlcvList, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update hoặc Insert (upsert) - dùng cho cập nhật nến chưa đóng
        /// </summary>
        Task UpsertAsync(Ohlcv ohlcv, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy OHLCV theo ticker, timeframe và khoảng thời gian
        /// </summary>
        Task<List<Ohlcv>> GetByTickerAndTimeRangeAsync(
            string ticker,
            string timeframe,
            DateTime fromTime,
            DateTime toTime,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy nến mới nhất của ticker với timeframe cụ thể
        /// </summary>
        Task<Ohlcv?> GetLatestAsync(
            string ticker,
            string timeframe,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy nhiều nến mới nhất (dùng cho chart)
        /// </summary>
        Task<List<Ohlcv>> GetLatestCandlesAsync(
            string ticker,
            string timeframe,
            int limit,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Kiểm tra xem đã có data cho ticker/timeframe/time chưa
        /// </summary>
        Task<bool> ExistsAsync(
            string ticker,
            string timeframe,
            DateTime time,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy tất cả ticker có data trong timeframe
        /// </summary>
        Task<List<string>> GetAvailableTickersAsync(
            string timeframe,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thời gian nến đầu tiên và cuối cùng của ticker
        /// </summary>
        Task<(DateTime? FirstTime, DateTime? LastTime)> GetTimeRangeAsync(
            string ticker,
            string timeframe,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Đếm số lượng nến
        /// </summary>
        Task<int> CountAsync(
            string ticker,
            string timeframe,
            DateTime? fromTime = null,
            DateTime? toTime = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa data cũ (dùng cho maintenance)
        /// </summary>
        Task DeleteOlderThanAsync(
            DateTime olderThan,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the oldest date for a ticker and timeframe
        /// </summary>
        Task<DateTime?> GetOldestDateAsync(
            string ticker,
            string timeframe,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the newest date for a ticker and timeframe
        /// </summary>
        Task<DateTime?> GetNewestDateAsync(
            string ticker,
            string timeframe,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete all OHLCV data by ticker and timeframe
        /// </summary>
        Task DeleteByTickerAndTimeframeAsync(
            string ticker,
            string timeframe,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete OHLCV data by ticker, timeframe and date range
        /// </summary>
        Task<int> DeleteByTickerTimeframeAndRangeAsync(
            string ticker,
            string timeframe,
            DateTime? fromTime = null,
            DateTime? toTime = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete OHLCV data for ALL tickers by timeframe and date range
        /// </summary>
        Task<int> DeleteByTimeframeAndRangeAsync(
            string timeframe,
            DateTime? fromTime = null,
            DateTime? toTime = null,
            CancellationToken cancellationToken = default);
    }
}