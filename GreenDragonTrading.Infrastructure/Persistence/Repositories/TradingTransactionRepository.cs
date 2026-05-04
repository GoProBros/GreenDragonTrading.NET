using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories;

public class TradingTransactionRepository : PostgreSqlGenericRepository<TradingTransaction>, ITradingTransactionRepository
{
    public TradingTransactionRepository(GdtPostgreSqlDbContext context) : base(context)
    {
    }

    public async Task<List<TradingTransaction>> GetByPortfolioIdsAsync(IEnumerable<int> portfolioIds, CancellationToken cancellationToken = default)
    {
        var ids = portfolioIds
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return [];
        }

        return await _dbSet
            .Where(x => ids.Contains(x.PortfolioId))
            .OrderByDescending(x => x.TransactionDate)
            .ToListAsync(cancellationToken);
    }
}
