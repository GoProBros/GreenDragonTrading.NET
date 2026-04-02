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
    }
}
