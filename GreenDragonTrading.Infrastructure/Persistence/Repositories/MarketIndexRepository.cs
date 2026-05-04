using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// EF Core implementation of <see cref="IMarketIndexRepository"/>.
    /// </summary>
    public class MarketIndexRepository(GdtPostgreSqlDbContext context)
        : PostgreSqlGenericRepository<MarketIndex>(context), IMarketIndexRepository
    {
        public async Task<MarketIndex?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Code.ToLower() == code.ToLower(), cancellationToken);
        }

        public async Task<List<MarketIndex>> GetByExchangeAsync(string exchangeCode, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(i => i.ExchangeCode == exchangeCode && i.Status == CommonStatus.Active)
                .OrderBy(i => i.Code)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<MarketIndex>> GetBenchmarkIndicesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(i => i.IsBenchmark && i.Status == CommonStatus.Active)
                .OrderBy(i => i.Code)
                .ToListAsync(cancellationToken);
        }

        public IQueryable<MarketIndex> GetQueryable()
        {
            return _dbSet.AsNoTracking();
        }
    }
}
