using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using GreenDragonTrading.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    public class UserPushTokenRepository(GdtPostgreSqlDbContext context) : IUserPushTokenRepository
    {
        private readonly GdtPostgreSqlDbContext _context = context;

        public async Task<List<UserPushToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => await _context.UserPushTokens
                .Where(t => t.UserId == userId)
                .ToListAsync(cancellationToken);

        public async Task<UserPushToken?> FindByUserAndTokenAsync(Guid userId, string token, CancellationToken cancellationToken = default)
            => await _context.UserPushTokens
                .FirstOrDefaultAsync(t => t.UserId == userId && t.Token == token, cancellationToken);

        public async Task AddAsync(UserPushToken entity, CancellationToken cancellationToken = default)
            => await _context.UserPushTokens.AddAsync(entity, cancellationToken);

        public void Update(UserPushToken entity)
            => _context.UserPushTokens.Update(entity);

        public async Task RemoveByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            var entity = await _context.UserPushTokens
                .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
            if (entity != null)
                _context.UserPushTokens.Remove(entity);
        }
    }
}
