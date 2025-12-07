using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces
{
    public interface ISymbolRepository : IPostgreSqlGenericRepository<Symbol>
    {
        Task<IEnumerable<string>> GetAllTickersAsync(CancellationToken cancellationToken = default);
    }
}
