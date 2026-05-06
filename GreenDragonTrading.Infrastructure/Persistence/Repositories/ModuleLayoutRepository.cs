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
        Guid userId,
        bool includeSystemDefaults = false,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        query = query.Where(l => l.ModuleType == moduleType);

        if (includeSystemDefaults)
        {
            // Admin/Staff: lấy layout cá nhân + system default
            query = query.Where(l => l.UserId == userId || l.IsSystemDefault);
        }
        else
        {
            // Normal user: chỉ lấy layout của chính mình
            query = query.Where(l => l.UserId == userId);
        }

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
        bool includeSystemDefaults = false,
        CancellationToken cancellationToken = default)
    {
        if (includeSystemDefaults)
        {
            // Admin/Staff: truy cập layout cá nhân + system default
            return await _dbSet
                .FirstOrDefaultAsync(
                    l => l.Id == id && (l.UserId == userId || l.IsSystemDefault),
                    cancellationToken);
        }

        // Normal user: chỉ truy cập layout của chính mình
        return await _dbSet
            .FirstOrDefaultAsync(
                l => l.Id == id && l.UserId == userId,
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
