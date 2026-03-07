using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for <see cref="MarketIndex"/> (metadata stored in PostgreSQL).
    /// </summary>
    public interface IMarketIndexRepository : IPostgreSqlGenericRepository<MarketIndex>
    {
        /// <summary>
        /// Returns a market index by its code (case-insensitive), or null if not found.
        /// </summary>
        Task<MarketIndex?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns all active market indices for a given exchange code.
        /// </summary>
        Task<List<MarketIndex>> GetByExchangeAsync(string exchangeCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns all benchmark indices (IsBenchmark = true).
        /// </summary>
        Task<List<MarketIndex>> GetBenchmarkIndicesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Exposes queryable for advanced filtering / pagination.
        /// </summary>
        IQueryable<MarketIndex> GetQueryable();
    }
}
