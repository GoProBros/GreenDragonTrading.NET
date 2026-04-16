using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces;

public interface ITradingTransactionRepository : IPostgreSqlGenericRepository<TradingTransaction>
{
    Task<List<TradingTransaction>> GetByPortfolioIdsAsync(IEnumerable<int> portfolioIds, CancellationToken cancellationToken = default);
}
