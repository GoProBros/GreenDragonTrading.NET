using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation cho ModuleLayout
/// </summary>
public class ModuleLayoutRepository : PostgreSqlGenericRepository<ModuleLayout>, IModuleLayoutRepository
{
    public ModuleLayoutRepository(GdtPostgreSqlDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task<List<ModuleLayout>> GetByModuleTypeAsync(
        ModuleType moduleType, 
        Guid? userId, 
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        // Lấy layout hệ thống hoặc layout cá nhân của user
        query = query.Where(l => 
            l.ModuleType == moduleType && 
            (l.IsSystemDefault || (userId.HasValue && l.UserId == userId.Value))
        );

        return await query
            .OrderByDescending(l => l.IsSystemDefault)
            .ThenByDescending(l => l.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ModuleLayout?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ModuleLayout?> GetByIdAndUserIdAsync(
        long id,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                l => l.Id == id && (l.UserId == userId || l.IsSystemDefault), 
                cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> IsOwnedByUserAsync(
        long id, 
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(l => l.Id == id && l.UserId == userId, cancellationToken);
    }

    /// <inheritdoc/>
    public void Delete(ModuleLayout layout)
    {
        _dbSet.Remove(layout);
    }
}
