using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    public class AlertRepository(GdtPostgreSqlDbContext context)
        : PostgreSqlGenericRepository<Alert>(context), IAlertRepository
    {
        public async Task<List<Alert>> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(cancellationToken);
        }

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

        public async Task<(List<Alert> Alerts, int TotalCount)> GetPaginatedByUserIdAsync(
            Guid userId,
            AlertType? type,
            ConditionType? condition,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var validPageIndex = pageIndex < 1 ? 1 : pageIndex;
            var validPageSize = pageSize < 1 ? 10 : pageSize;

            var query = _dbSet
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .AsQueryable();

            if (type.HasValue)
            {
                query = query.Where(a => a.Type == type.Value);
            }

            if (condition.HasValue)
            {
                query = query.Where(a => a.Condition == condition.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var alerts = await query
                .OrderByDescending(a => a.CreatedAt)
                .ThenByDescending(a => a.Id)
                .Skip((validPageIndex - 1) * validPageSize)
                .Take(validPageSize)
                .ToListAsync(cancellationToken);

            return (alerts, totalCount);
        }
    }
}
