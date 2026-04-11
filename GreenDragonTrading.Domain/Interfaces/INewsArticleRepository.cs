using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces
{
    public interface INewsArticleRepository : IPostgreSqlGenericRepository<NewsArticle>
    {
        Task<List<string>> GetExistingLinksAsync(IEnumerable<string> links, CancellationToken cancellationToken = default);

        Task<(List<NewsArticle> Articles, int TotalCount)> GetPaginatedAsync(
            string? search,
            string? ticker,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
