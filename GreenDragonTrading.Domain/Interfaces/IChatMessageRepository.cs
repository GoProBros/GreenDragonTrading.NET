using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces
{
    public interface IChatMessageRepository : IPostgreSqlGenericRepository<ChatMessage>
    {
        Task<(List<ChatMessage> Messages, int TotalCount)> GetMessagesBySessionIdAsync(
            int sessionId,
            int pageIndex = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default);
        Task<List<ChatMessage>> GetAllMessagesBySessionIdAsync(int sessionId, CancellationToken cancellationToken = default);
        Task<List<ChatMessage>> GetRecentMessagesAsync(int sessionId, int limit, CancellationToken cancellationToken = default);
        Task<List<ChatMessage>> GetMessagesAfterIdAsync(int sessionId, int afterMessageId, CancellationToken cancellationToken = default);
    }
}
