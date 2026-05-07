using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    public class SubscriptionRepository : PostgreSqlGenericRepository<Subscription>, ISubscriptionRepository
    {
        public SubscriptionRepository(GdtPostgreSqlDbContext context) : base(context)
        {
        }

        public async Task<Subscription?> GetByLevelAsync(int levelOrder, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Subscription>()
                .FirstOrDefaultAsync(s => s.LevelOrder.HasValue && (int)s.LevelOrder.Value == levelOrder, cancellationToken);
        }

        public async Task<Subscription?> GetHighestActiveAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<Subscription>()
                .Where(s => s.IsActive == CommonStatus.Active)
                .OrderByDescending(s => s.LevelOrder ?? (SubscriptionLevel)(-1))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Subscription?> GetActiveFreeSubscriptionAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<Subscription>()
                .Where(s => s.IsActive == CommonStatus.Active && s.IsFree)
                .OrderByDescending(s => s.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Subscription?> GetActiveAdminSubscriptionAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<Subscription>()
                .Where(s => s.IsActive == CommonStatus.Active && s.IsAdmin)
                .OrderByDescending(s => s.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
