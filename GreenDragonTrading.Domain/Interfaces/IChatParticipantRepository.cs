using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces
{
    public interface IChatParticipantRepository : IPostgreSqlGenericRepository<ChatParticipant>
    {
        Task<List<ChatParticipant>> GetParticipantsBySessionIdAsync(int sessionId, CancellationToken cancellationToken = default);
        Task<int> GetParticipantCountBySessionIdAsync(int sessionId, CancellationToken cancellationToken = default);
        Task<bool> IsUserParticipantAsync(int sessionId, Guid userId, CancellationToken cancellationToken = default);
    }
}
