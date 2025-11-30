using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    public class SymbolRepository(GdtPostgreSqlDbContext context) : PostgreSqlGenericRepository<Symbol>(context), ISymbolRepository
    {
    }
}
