using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    public class ProactiveAlertLayerBSettingRepository(GdtPostgreSqlDbContext context)
        : PostgreSqlGenericRepository<ProactiveAlertLayerBSetting>(context), IProactiveAlertLayerBSettingRepository
    {
    }
}
