using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces;

public interface IPortfolioRepository : IPostgreSqlGenericRepository<Portfolio>
{
    Task<List<Portfolio>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Portfolio?> GetByIdAndUserIdAsync(int id, Guid userId, CancellationToken cancellationToken = default);
}
