using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories;

public class PortfolioRepository : PostgreSqlGenericRepository<Portfolio>, IPortfolioRepository
{
    public PortfolioRepository(GdtPostgreSqlDbContext context) : base(context)
    {
    }

    public async Task<List<Portfolio>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Portfolio?> GetByIdAndUserIdAsync(int id, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
    }
}
