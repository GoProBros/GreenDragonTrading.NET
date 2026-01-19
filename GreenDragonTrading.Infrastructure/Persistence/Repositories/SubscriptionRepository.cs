using GreenDragonTrading.Domain.Entities;
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
                .FirstOrDefaultAsync(s => (int)s.LevelOrder == levelOrder, cancellationToken);
        }
    }
}
