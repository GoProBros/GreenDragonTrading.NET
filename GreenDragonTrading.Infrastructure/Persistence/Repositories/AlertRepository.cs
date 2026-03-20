using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    public class AlertRepository(GdtPostgreSqlDbContext context)
        : PostgreSqlGenericRepository<Alert>(context), IAlertRepository
    {
        public async Task<List<Alert>> GetActiveAlertsByIdsAsync(
            IEnumerable<int> ids,
            CancellationToken cancellationToken = default)
        {
            var idList = ids.Distinct().ToList();
            if (idList.Count == 0)
            {
                return [];
            }

            return await _dbSet
                .Where(a => idList.Contains(a.Id) && a.IsActive && !a.IsTriggered)
                .ToListAsync(cancellationToken);
        }
    }
}
