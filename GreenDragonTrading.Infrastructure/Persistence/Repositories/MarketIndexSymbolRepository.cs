using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// EF Core implementation of <see cref="IMarketIndexSymbolRepository"/>.
    /// </summary>
    public class MarketIndexSymbolRepository(GdtPostgreSqlDbContext context)
        : PostgreSqlGenericRepository<MarketIndexSymbol>(context), IMarketIndexSymbolRepository
    {
        public async Task<List<MarketIndexSymbol>> GetActiveByIndexAsync(
            string indexCode,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(m => m.IndexCode == indexCode && m.IsActive)
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.Ticker)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<MarketIndexSymbol>> GetActiveByTickerAsync(
            string ticker,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(m => m.Ticker == ticker && m.IsActive)
                .OrderBy(m => m.IndexCode)
                .ToListAsync(cancellationToken);
        }

        public async Task<MarketIndexSymbol?> GetByIndexAndTickerAsync(
            string indexCode,
            string ticker,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IndexCode == indexCode && m.Ticker == ticker, cancellationToken);
        }

        public async Task BulkUpsertAsync(
            IEnumerable<MarketIndexSymbol> constituents,
            CancellationToken cancellationToken = default)
        {
            var list = constituents.ToList();
            if (list.Count == 0) return;

            var now = DateTime.UtcNow;

            foreach (var item in list)
            {
                var existing = await _dbSet.FindAsync([item.IndexCode, item.Ticker], cancellationToken);
                if (existing is null)
                {
                    item.CreatedAt = now;
                    await _dbSet.AddAsync(item, cancellationToken);
                }
                else
                {
                    existing.Weight = item.Weight;
                    existing.IsActive = item.IsActive;
                    existing.AddedDate = item.AddedDate;
                    existing.RemovedDate = item.RemovedDate;
                    existing.DisplayOrder = item.DisplayOrder;
                    existing.UpdatedAt = now;
                    _dbSet.Update(existing);
                }
            }
        }

        public IQueryable<MarketIndexSymbol> GetQueryable()
        {
            return _dbSet.AsNoTracking();
        }
    }
}
