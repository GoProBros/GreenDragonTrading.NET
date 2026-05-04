using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    public class WorkspaceRepository : PostgreSqlGenericRepository<Workspace>, IWorkspaceRepository
    {
        public WorkspaceRepository(GdtPostgreSqlDbContext context) : base(context)
        {
        }
        public async Task<List<Workspace>> GetWorkspaceByUserIdAsync(
            Guid userId,
            WorkspaceType? type = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Set<Workspace>()
                .Where(w => w.UserId == userId);

            if (type.HasValue)
            {
                query = query.Where(w => w.Type == type.Value);
            }

            return await query
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Workspace?> GetByShareCodeAsync(string shareCode, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Workspace>()
                .FirstOrDefaultAsync(w => w.ShareCode == shareCode, cancellationToken);
        }

        public async Task<bool> ShareCodeExistsAsync(string shareCode, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Workspace>()
                .AnyAsync(w => w.ShareCode == shareCode, cancellationToken);
        }

        public async Task<Workspace?> GetSystemDefaultWorkspaceAsync(
            WorkspaceType? type = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Set<Workspace>()
                .Where(w => w.UserId == null && w.IsDefault);

            if (type.HasValue)
            {
                query = query.Where(w => w.Type == type.Value);
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<Workspace>> GetSystemWorkspacesAsync(
            WorkspaceType? type = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Set<Workspace>()
                .Where(w => w.UserId == null);

            if (type.HasValue)
            {
                query = query.Where(w => w.Type == type.Value);
            }

            return await query
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
