using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces;

public interface IMacroeconomicDataRepository : IPostgreSqlGenericRepository<MacroeconomicData>
{
}