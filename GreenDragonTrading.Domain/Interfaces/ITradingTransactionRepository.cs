using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces;

public interface ITradingTransactionRepository : IPostgreSqlGenericRepository<TradingTransaction>
{
}
