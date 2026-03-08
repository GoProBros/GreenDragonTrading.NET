namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    using GreenDragonTrading.Domain.Entities;
    using GreenDragonTrading.Domain.Interfaces;
    using Microsoft.EntityFrameworkCore;

    public class ChatMessageRepository : PostgreSqlGenericRepository<ChatMessage>, IChatMessageRepository
    {
        public ChatMessageRepository(GdtPostgreSqlDbContext context) : base(context)
        {
        }

        public async Task<(List<ChatMessage> Messages, int TotalCount)> GetMessagesBySessionIdAsync(
            int sessionId,
            int pageIndex = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Set<ChatMessage>()
                .Where(m => m.SessionId == sessionId && !m.IsDeleted);

            var totalCount = await query.CountAsync(cancellationToken);

            var messages = await query
                .OrderBy(m => m.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (messages, totalCount);
        }

        public async Task<List<ChatMessage>> GetAllMessagesBySessionIdAsync(int sessionId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ChatMessage>()
                .Where(m => m.SessionId == sessionId && !m.IsDeleted)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ChatMessage>> GetRecentMessagesAsync(int sessionId, int limit, CancellationToken cancellationToken = default)
        {
            var messages = await _context.Set<ChatMessage>()
                .Where(m => m.SessionId == sessionId && !m.IsDeleted)
                .OrderByDescending(m => m.CreatedAt)
                .Take(limit)
                .ToListAsync(cancellationToken);

            messages.Reverse();
            return messages;
        }

        public async Task<List<ChatMessage>> GetMessagesAfterIdAsync(int sessionId, int afterMessageId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ChatMessage>()
                .Where(m => m.SessionId == sessionId && !m.IsDeleted && m.Id > afterMessageId)
                .OrderBy(m => m.Id)
                .ToListAsync(cancellationToken);
        }
    }
}
