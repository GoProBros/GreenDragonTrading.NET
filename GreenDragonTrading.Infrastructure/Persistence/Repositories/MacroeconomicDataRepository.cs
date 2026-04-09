using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using GreenDragonTrading.Infrastructure.Persistence;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories;

public class MacroeconomicDataRepository : PostgreSqlGenericRepository<MacroeconomicData>, IMacroeconomicDataRepository
{
    public MacroeconomicDataRepository(GdtPostgreSqlDbContext context) : base(context)
    {
    }
}