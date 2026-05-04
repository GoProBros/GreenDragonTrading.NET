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

        public async Task<(List<CorporateAction> Items, int TotalCount)> GetPaginatedAsync(
            string? search,
            string? symbol,
            int? eventType,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    (x.Title != null && x.Title.ToLower().Contains(keyword))
                    || (x.Content != null && x.Content.ToLower().Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(symbol))
            {
                var normalizedSymbol = symbol.Trim().ToUpper();
                query = query.Where(x => x.Ticker == normalizedSymbol);
            }

            if (eventType.HasValue)
            {
                query = query.Where(x => x.EventType == eventType.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var fallbackDate = DateTimeOffset.UnixEpoch;
            var items = await query
                .OrderByDescending(x => x.ExRightsDate ?? fallbackDate)
                .ThenByDescending(x => x.RecordDate ?? fallbackDate)
                .ThenByDescending(x => x.ActionDate ?? fallbackDate)
                .ThenByDescending(x => x.EventId)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
