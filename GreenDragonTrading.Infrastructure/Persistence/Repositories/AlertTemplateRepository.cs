using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    public class AlertTemplateRepository(GdtPostgreSqlDbContext context)
        : PostgreSqlGenericRepository<AlertTemplate>(context), IAlertTemplateRepository
    {
        public async Task<AlertTemplate?> GetActiveByTypeAndConditionAsync(
            AlertType type,
            ConditionType condition,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.IsActive && x.Type == type && x.Condition == condition,
                    cancellationToken);
        }

        public async Task<AlertTemplate?> GetDefaultAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IsActive && x.IsDefault, cancellationToken);
        }
    }
}
