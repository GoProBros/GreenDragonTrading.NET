using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Domain.Interfaces
{
    public interface ITransactionRepository : IPostgreSqlGenericRepository<Transaction>
    {
        Task<Transaction?> GetByOrderCodeAsync(long orderCode, CancellationToken cancellationToken = default);
        Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<(List<Transaction> Transactions, int TotalCount)> GetPaginatedByUserIdAsync(
            Guid userId,
            TransactionStatus? status,
            PaymentType? paymentProvider,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken = default);
        Task<IEnumerable<Transaction>> GetExpiredPendingMomoAsync(int expirationMinutes, CancellationToken cancellationToken = default);
    }
}
