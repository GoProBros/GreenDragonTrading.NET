using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces
{
    public interface INewsArticleRepository : IPostgreSqlGenericRepository<NewsArticle>
    {
        Task<List<string>> GetExistingLinksAsync(IEnumerable<string> links, CancellationToken cancellationToken = default);
    }
}
