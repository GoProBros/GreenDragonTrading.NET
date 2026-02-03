using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using GreenDragonTrading.Infrastructure.Persistence;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for AnalysisReport
/// </summary>
public class AnalysisReportRepository : PostgreSqlGenericRepository<AnalysisReport>, IAnalysisReportRepository
{
    public AnalysisReportRepository(GdtPostgreSqlDbContext context) : base(context)
    {
    }
}
