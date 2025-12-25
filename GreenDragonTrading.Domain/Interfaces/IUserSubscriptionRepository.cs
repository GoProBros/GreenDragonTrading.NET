using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces
{
    public interface IUserSubscriptionRepository : IPostgreSqlGenericRepository<UserSubscription>
    {
        /// <summary>
        /// Get active subscription for a user
        /// </summary>
        Task<UserSubscription?> GetActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Check if user has active subscription
        /// </summary>
        Task<bool> HasActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
