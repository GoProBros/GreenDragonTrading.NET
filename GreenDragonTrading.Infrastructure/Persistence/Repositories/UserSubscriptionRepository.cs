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
    }
}
