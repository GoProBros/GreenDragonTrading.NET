using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces
{
    public interface IUserSubscriptionRepository : IPostgreSqlGenericRepository<UserSubscription>
    {
        Task<UserSubscription?> GetActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> HasActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<UserSubscription?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<List<UserSubscription>> GetAllActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get the active subscription with highest level for a user
        /// </summary>
        Task<UserSubscription?> GetHighestLevelActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get the maximum end date among all active subscriptions with specific subscription id
        /// </summary>
        Task<DateTimeOffset?> GetMaxEndDateBySubscriptionIdAsync(Guid userId, int subscriptionId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Mark all active subscriptions as Upgraded
        /// </summary>
        Task MarkAllActiveAsUpgradedAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
