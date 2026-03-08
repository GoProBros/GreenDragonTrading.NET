using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    public class ChatParticipantRepository : PostgreSqlGenericRepository<ChatParticipant>, IChatParticipantRepository
    {
        public ChatParticipantRepository(GdtPostgreSqlDbContext context) : base(context)
        {
        }

        public async Task<List<ChatParticipant>> GetParticipantsBySessionIdAsync(int sessionId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ChatParticipant>()
                .Include(p => p.User)
                .Where(p => p.SessionId == sessionId)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetParticipantCountBySessionIdAsync(int sessionId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ChatParticipant>()
                .CountAsync(p => p.SessionId == sessionId, cancellationToken);
        }

        public async Task<bool> IsUserParticipantAsync(int sessionId, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ChatParticipant>()
                .AnyAsync(p => p.SessionId == sessionId && p.UserId == userId, cancellationToken);
        }
    }
}
