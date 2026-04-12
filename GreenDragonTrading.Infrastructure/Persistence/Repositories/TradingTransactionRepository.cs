using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories;

public class TradingTransactionRepository : PostgreSqlGenericRepository<TradingTransaction>, ITradingTransactionRepository
{
    public TradingTransactionRepository(GdtPostgreSqlDbContext context) : base(context)
    {
    }
}
