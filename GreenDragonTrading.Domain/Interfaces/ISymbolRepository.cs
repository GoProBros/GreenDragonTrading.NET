using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Domain.Interfaces
{
    public interface ISymbolRepository : IPostgreSqlGenericRepository<Symbol>
    {
        Task<IEnumerable<string>> GetAllTickersAsync(CancellationToken cancellationToken = default);
                
        Task<(IEnumerable<Symbol> Symbols, int TotalCount)> GetSymbolsAsync(
            int pageIndex,
            int pageSize,
            SymbolType? type = null,
            string? exchange = null,
            string? sector = null,
            CancellationToken cancellationToken = default);

        Task<(IEnumerable<Symbol> Symbols, int TotalCount)> SearchSymbolsAsync(
            string query,
            bool isTickerOnly,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
