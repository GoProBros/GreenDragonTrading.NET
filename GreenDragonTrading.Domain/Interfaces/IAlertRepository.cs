using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Domain.Interfaces
{
    public interface IAlertRepository : IPostgreSqlGenericRepository<Alert>
    {
        Task<List<Alert>> GetActiveAlertsByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
        Task<List<Alert>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<(List<Alert> Alerts, int TotalCount)> GetPaginatedByUserIdAsync(
            Guid userId,
            AlertType? type,
            ConditionType? condition,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
