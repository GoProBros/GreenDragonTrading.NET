using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    public class ProactiveAlertTraceRepository(GdtPostgreSqlDbContext context)
        : PostgreSqlGenericRepository<ProactiveAlertTrace>(context), IProactiveAlertTraceRepository
    {
        public IQueryable<ProactiveAlertTrace> GetQueryable()
        {
            return _dbSet.AsQueryable();
        }
    }
}
