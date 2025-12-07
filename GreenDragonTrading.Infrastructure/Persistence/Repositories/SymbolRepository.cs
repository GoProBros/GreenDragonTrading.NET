using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    public class SymbolRepository(GdtPostgreSqlDbContext context) : PostgreSqlGenericRepository<Symbol>(context), ISymbolRepository
    {
        public async Task<IEnumerable<string>> GetAllTickersAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Select(s => s.Ticker)
                .ToListAsync(cancellationToken);
        }
    }
}
