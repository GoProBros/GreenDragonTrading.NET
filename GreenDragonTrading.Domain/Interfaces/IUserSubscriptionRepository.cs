using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces
{
    public interface IUserSubscriptionRepository : IPostgreSqlGenericRepository<UserSubscription>
    {
        Task<UserSubscription?> GetActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> HasActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<UserSubscription?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<List<UserSubscription>> GetAllActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<DateTimeOffset?> GetMaxEndDateBySubscriptionIdAsync(Guid userId, int subscriptionId, CancellationToken cancellationToken = default);
        Task MarkAllActiveAsUpgradedAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
