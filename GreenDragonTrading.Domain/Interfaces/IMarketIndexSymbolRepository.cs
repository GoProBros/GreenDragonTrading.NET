using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for <see cref="MarketIndexSymbol"/> (constituent mapping).
    /// </summary>
    public interface IMarketIndexSymbolRepository : IPostgreSqlGenericRepository<MarketIndexSymbol>
    {
        /// <summary>
        /// Returns all active constituent symbols for a given index code.
        /// </summary>
        Task<List<MarketIndexSymbol>> GetActiveByIndexAsync(string indexCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns all index memberships (active) for a given ticker.
        /// </summary>
        Task<List<MarketIndexSymbol>> GetActiveByTickerAsync(string ticker, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a specific index–symbol record, or null if it does not exist.
        /// </summary>
        Task<MarketIndexSymbol?> GetByIndexAndTickerAsync(string indexCode, string ticker, CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk-upserts constituent mappings (insert new / update existing by composite PK).
        /// </summary>
        Task BulkUpsertAsync(IEnumerable<MarketIndexSymbol> constituents, CancellationToken cancellationToken = default);

        /// <summary>
        /// Exposes queryable for advanced filtering / pagination.
        /// </summary>
        IQueryable<MarketIndexSymbol> GetQueryable();
    }
}
