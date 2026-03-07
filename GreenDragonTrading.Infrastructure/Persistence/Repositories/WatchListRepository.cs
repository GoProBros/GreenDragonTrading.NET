namespace GreenDragonTrading.Infrastructure.Persistence.Repositories;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
public class WatchListRepository : PostgreSqlGenericRepository<WatchList>, IWatchListRepository
{
    public WatchListRepository(GdtPostgreSqlDbContext context) : base(context)
    {
    }

    public async Task<List<WatchList>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(w => w.UserId == userId && w.Status == CommonStatus.Active)
            .OrderByDescending(w => w.UpdatedAt ?? w.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<WatchList?> GetByIdAndUserIdAsync(int id, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId, cancellationToken);
    }

    public async Task<bool> IsOwnedByUserAsync(int id, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(w => w.Id == id && w.UserId == userId, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(
            w => w.UserId == userId && w.Name == name && w.Status == CommonStatus.Active, 
            cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(Guid userId, string name, int excludeId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(
            w => w.UserId == userId && w.Name == name && w.Id != excludeId && w.Status == CommonStatus.Active, 
            cancellationToken);
    }

    public void Delete(WatchList watchList)
    {
        _dbSet.Remove(watchList);
    }
}
