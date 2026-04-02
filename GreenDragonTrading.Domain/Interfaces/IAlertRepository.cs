using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces
{
    public interface IAlertRepository : IPostgreSqlGenericRepository<Alert>
    {
        Task<List<Alert>> GetActiveAlertsByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
        Task<List<Alert>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
