using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    public class WorkspaceRepository : PostgreSqlGenericRepository<Workspace>, IWorkspaceRepository
    {
        public WorkspaceRepository(GdtPostgreSqlDbContext context) : base(context)
        {
        }
        public async Task<List<Workspace>> GetWorkspaceByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Workspace>()
                .Where(w => w.UserId == userId)
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

        public async Task<Workspace?> GetSystemDefaultWorkspaceAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<Workspace>()
                .FirstOrDefaultAsync(w => w.UserId == null && w.IsDefault, cancellationToken);
        }
    }
}
