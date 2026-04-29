using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    public class CorporateActionRepository : PostgreSqlGenericRepository<CorporateAction>, ICorporateActionRepository
    {
        public CorporateActionRepository(GdtPostgreSqlDbContext context) : base(context)
        {
        }

        public async Task<List<CorporateAction>> GetByEventIdsAsync(IEnumerable<int> eventIds, CancellationToken cancellationToken = default)
        {
            var ids = eventIds
                .Distinct()
                .ToList();

            if (ids.Count == 0)
            {
                return new List<CorporateAction>();
            }

            return await _dbSet
                .Where(x => ids.Contains(x.EventId))
                .ToListAsync(cancellationToken);
        }
    }
}
