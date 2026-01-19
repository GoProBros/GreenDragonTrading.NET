using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces
{
    public interface IUserSubscriptionRepository : IPostgreSqlGenericRepository<UserSubscription>
    {
        Task<UserSubscription?> GetActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> HasActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<UserSubscription?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
