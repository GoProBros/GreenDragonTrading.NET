using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    public class SectorRepository(GdtPostgreSqlDbContext context) : PostgreSqlGenericRepository<Sector>(context), ISectorRepository
    {
    }
}
