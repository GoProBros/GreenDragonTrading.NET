using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces
{
    public interface ICorporateActionRepository : IPostgreSqlGenericRepository<CorporateAction>
    {
        Task<List<CorporateAction>> GetByEventIdsAsync(IEnumerable<int> eventIds, CancellationToken cancellationToken = default);
    }
}
