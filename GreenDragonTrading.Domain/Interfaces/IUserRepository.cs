using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces
{
    public interface IUserRepository : IPostgreSqlGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
        Task<User?> FindByPhoneOrEmailAsync(string input, CancellationToken cancellationToken = default);
    }
}