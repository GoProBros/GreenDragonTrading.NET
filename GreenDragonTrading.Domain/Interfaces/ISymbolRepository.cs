using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces
{
    public interface ISymbolRepository : IPostgreSqlGenericRepository<Symbol>
    {
    }
}
