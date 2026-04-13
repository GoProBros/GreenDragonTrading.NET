using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    public class TransactionRepository : PostgreSqlGenericRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(GdtPostgreSqlDbContext context) : base(context)
        {
        }

        public async Task<Transaction?> GetByOrderCodeAsync(long orderCode, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Transaction>()
                .Include(t => t.Subscription)
                .FirstOrDefaultAsync(t => t.OrderCode == orderCode, cancellationToken);
        }

        public async Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Transaction>()
                .Include(t => t.Subscription)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<(List<Transaction> Transactions, int TotalCount)> GetPaginatedByUserIdAsync(
            Guid userId,
            TransactionStatus? status,
            PaymentType? paymentProvider,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var validPageIndex = pageIndex < 1 ? 1 : pageIndex;
            var validPageSize = pageSize < 1 ? 10 : pageSize;

            var query = _context.Set<Transaction>()
                .AsNoTracking()
                .Include(t => t.Subscription)
                .Where(t => t.UserId == userId)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            if (paymentProvider.HasValue)
            {
                query = query.Where(t => t.PaymentProvider == paymentProvider.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var transactions = await query
                .OrderByDescending(t => t.CreatedAt)
                .ThenByDescending(t => t.OrderCode)
                .Skip((validPageIndex - 1) * validPageSize)
                .Take(validPageSize)
                .ToListAsync(cancellationToken);

            return (transactions, totalCount);
        }

        public async Task<IEnumerable<Transaction>> GetExpiredPendingMomoAsync(
            int expirationMinutes,
            CancellationToken cancellationToken = default)
        {
            var cutoff = DateTimeOffset.UtcNow.AddMinutes(-expirationMinutes);

            return await _context.Set<Transaction>()
                .Where(t =>
                    t.PaymentProvider == PaymentType.Momo &&
                    t.Status == TransactionStatus.Pending &&
                    t.CreatedAt < cutoff)
                .ToListAsync(cancellationToken);
        }
    }
}
