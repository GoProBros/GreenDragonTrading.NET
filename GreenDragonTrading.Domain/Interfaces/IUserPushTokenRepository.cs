using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces
{
    public interface IUserPushTokenRepository
    {
        Task<List<UserPushToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<UserPushToken?> FindByUserAndTokenAsync(Guid userId, string token, CancellationToken cancellationToken = default);
        Task AddAsync(UserPushToken entity, CancellationToken cancellationToken = default);
        void Update(UserPushToken entity);
        Task RemoveByTokenAsync(string token, CancellationToken cancellationToken = default);
    }
}
