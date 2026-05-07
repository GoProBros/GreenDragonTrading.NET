using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces
{
    public interface IProactiveAlertTraceRepository : IPostgreSqlGenericRepository<ProactiveAlertTrace>
    {
        IQueryable<ProactiveAlertTrace> GetQueryable();
    }
}
