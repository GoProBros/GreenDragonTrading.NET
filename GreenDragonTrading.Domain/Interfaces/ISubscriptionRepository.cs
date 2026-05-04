using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces
{
    public interface ISubscriptionRepository : IPostgreSqlGenericRepository<Subscription>
    {
        Task<Subscription?> GetByLevelAsync(int levelOrder, CancellationToken cancellationToken = default);
        Task<Subscription?> GetHighestActiveAsync(CancellationToken cancellationToken = default);
        Task<Subscription?> GetActiveFreeSubscriptionAsync(CancellationToken cancellationToken = default);
        Task<Subscription?> GetActiveAdminSubscriptionAsync(CancellationToken cancellationToken = default);
    }
}
