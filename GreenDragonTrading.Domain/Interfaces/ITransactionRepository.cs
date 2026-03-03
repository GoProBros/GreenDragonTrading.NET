using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces
{
    public interface ITransactionRepository : IPostgreSqlGenericRepository<Transaction>
    {
        Task<Transaction?> GetByOrderCodeAsync(long orderCode, CancellationToken cancellationToken = default);
        Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns all Momo transactions that are still Pending but were created
        /// more than <paramref name="expirationMinutes"/> minutes ago.
        /// </summary>
        Task<IEnumerable<Transaction>> GetExpiredPendingMomoAsync(int expirationMinutes, CancellationToken cancellationToken = default);
    }
}
