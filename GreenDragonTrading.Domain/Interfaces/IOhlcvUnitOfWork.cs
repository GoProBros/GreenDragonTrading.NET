namespace GreenDragonTrading.Domain.Interfaces
{
    /// <summary>
    /// Unit of Work interface cho OHLCV operations
    /// </summary>
    public interface IOhlcvUnitOfWork
    {
        /// <summary>
        /// Repository để access OHLCV data
        /// </summary>
        IOhlcvRepository Ohlcv { get; }

        /// <summary>
        /// Commit tất cả changes vào database
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}