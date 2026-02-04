using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using GreenDragonTrading.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    public class UserSubscriptionRepository : PostgreSqlGenericRepository<UserSubscription>, IUserSubscriptionRepository
    {
        public UserSubscriptionRepository(GdtPostgreSqlDbContext context) : base(context)
        {
        }

        public async Task<UserSubscription?> GetActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            
            return await _context.Set<UserSubscription>()
                .Include(us => us.Subscription)
                .Where(us => us.UserId == userId 
                    && us.Status == SubscriptionStatus.Active 
                    && us.EndDate > now)
                .OrderByDescending(us => us.Subscription.LevelOrder)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> HasActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            
            return await _context.Set<UserSubscription>()
                .AnyAsync(us => us.UserId == userId 
                    && us.Status == SubscriptionStatus.Active 
                    && us.EndDate > now, 
                    cancellationToken);
        }

        public async Task<UserSubscription?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<UserSubscription>()
                .Include(us => us.Subscription)
                .FirstOrDefaultAsync(us => us.UserId == userId, cancellationToken);
        }

        public async Task<List<UserSubscription>> GetAllActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;

            return await _context.Set<UserSubscription>()
                .Include(us => us.Subscription)
                .Where(us => us.UserId == userId
                    && us.Status == SubscriptionStatus.Active
                    && us.EndDate > now)
                .ToListAsync(cancellationToken);
        }

        public async Task<DateTimeOffset?> GetMaxEndDateBySubscriptionIdAsync(Guid userId, int subscriptionId, CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;

            var maxEndDate = await _context.Set<UserSubscription>()
                .Where(us => us.UserId == userId
                    && us.SubscriptionId == subscriptionId
                    && us.Status == SubscriptionStatus.Active
                    && us.EndDate > now)
                .MaxAsync(us => (DateTimeOffset?)us.EndDate, cancellationToken);

            return maxEndDate;
        }

        public async Task MarkAllActiveAsUpgradedAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;

            await _context.Set<UserSubscription>()
                .Where(us => us.UserId == userId
                    && us.Status == SubscriptionStatus.Active
                    && us.EndDate > now)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(us => us.Status, SubscriptionStatus.Upgraded), cancellationToken);
        }
    }
}
