using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    public class UserRepository : PostgreSqlGenericRepository<User>, IUserRepository
    {
        public UserRepository(GdtPostgreSqlDbContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .AnyAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<User?> FindByPhoneOrEmailAsync(string input, CancellationToken cancellationToken = default)
        {
            var trimmed = input.Trim();
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    u => u.Email.ToLower() == trimmed.ToLower() || u.PhoneNumber == trimmed,
                    cancellationToken);
        }

        public async Task<List<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(u => u.Status == Domain.Enums.CommonStatus.Active)
                .ToListAsync(cancellationToken);
        }
    }
}