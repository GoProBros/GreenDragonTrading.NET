using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces
{
    public interface ICorporateActionRepository : IPostgreSqlGenericRepository<CorporateAction>
    {
        Task<List<CorporateAction>> GetByEventIdsAsync(IEnumerable<int> eventIds, CancellationToken cancellationToken = default);
        Task<(List<CorporateAction> Items, int TotalCount)> GetPaginatedAsync(
            string? search,
            string? symbol,
            int? eventType,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
