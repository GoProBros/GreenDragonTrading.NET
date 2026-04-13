using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    public class NewsArticleRepository : PostgreSqlGenericRepository<NewsArticle>, INewsArticleRepository
    {
        public NewsArticleRepository(GdtPostgreSqlDbContext context) : base(context)
        {
        }

        public async Task<NewsArticle?> GetByIdWithTagsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(x => x.ArticleTags)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<List<string>> GetExistingLinksAsync(IEnumerable<string> links, CancellationToken cancellationToken = default)
        {
            var normalizedLinks = links
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (normalizedLinks.Count == 0)
            {
                return new List<string>();
            }

            return await _dbSet
                .Where(x => x.Link != null && normalizedLinks.Contains(x.Link))
                .Select(x => x.Link!)
                .ToListAsync(cancellationToken);
        }

        public async Task<(List<NewsArticle> Articles, int TotalCount)> GetPaginatedAsync(
            string? search,
            string? ticker,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(x => x.ArticleTags)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    (x.Title != null && x.Title.ToLower().Contains(keyword))
                    || (x.Summary != null && x.Summary.ToLower().Contains(keyword))
                    || (x.Content != null && x.Content.ToLower().Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(ticker))
            {
                var normalizedTicker = ticker.Trim().ToUpper();
                query = query.Where(x => x.ArticleTags.Any(t => t.Ticker == normalizedTicker));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var articles = await query
                .OrderByDescending(x => x.PublishedAt)
                .ThenByDescending(x => x.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (articles, totalCount);
        }
    }
}
