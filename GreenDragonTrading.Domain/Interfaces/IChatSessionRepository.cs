using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces
{
    public interface IChatSessionRepository : IPostgreSqlGenericRepository<ChatSession>
    {
        Task<List<ChatSession>> GetSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<ChatSession?> GetSessionWithMessagesAsync(int sessionId, CancellationToken cancellationToken = default);
        Task<List<ChatSession>> GetAiSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<List<ChatSession>> GetSessionsWithParticipantsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<ChatSession?> GetDirectSessionBetweenUsersAsync(Guid userAId, Guid userBId, CancellationToken cancellationToken = default);
        Task<List<ChatSession>> GetDirectSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<ChatSession?> GetSystemSessionByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
