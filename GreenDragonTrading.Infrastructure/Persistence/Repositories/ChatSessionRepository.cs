using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    public class ChatSessionRepository : PostgreSqlGenericRepository<ChatSession>, IChatSessionRepository
    {
        public ChatSessionRepository(GdtPostgreSqlDbContext context) : base(context)
        {
        }

        public async Task<List<ChatSession>> GetSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ChatSession>()
                .Where(s => s.CreatedBy == userId && s.Status == CommonStatus.Active)
                .OrderByDescending(s => s.UpdatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<ChatSession?> GetSessionWithMessagesAsync(int sessionId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ChatSession>()
                .Include(s => s.Messages.Where(m => !m.IsDeleted))
                .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
        }

        public async Task<List<ChatSession>> GetAiSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ChatSession>()
                .Where(s => s.CreatedBy == userId 
                    && s.SessionType == ChatSessionType.AI 
                    && s.Status == CommonStatus.Active)
                .OrderByDescending(s => s.UpdatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ChatSession>> GetSessionsWithParticipantsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ChatSession>()
                .Include(s => s.Participants)
                    .ThenInclude(p => p.User)
                .Where(s => s.Status == CommonStatus.Active 
                    && s.Participants.Any(p => p.UserId == userId))
                .OrderByDescending(s => s.UpdatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
