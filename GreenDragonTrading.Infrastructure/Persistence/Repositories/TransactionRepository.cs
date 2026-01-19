using GreenDragonTrading.Domain.Entities;
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
    }
}
