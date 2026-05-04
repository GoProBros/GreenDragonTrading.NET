using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces;

public interface IWatchListRepository : IPostgreSqlGenericRepository<WatchList>
{
    Task<List<WatchList>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    new Task<WatchList?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<WatchList?> GetByIdAndUserIdAsync(int id, Guid userId, CancellationToken cancellationToken = default);

    Task<bool> IsOwnedByUserAsync(int id, Guid userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(Guid userId, string name, int excludeId, CancellationToken cancellationToken = default);

    void Delete(WatchList watchList);
}
