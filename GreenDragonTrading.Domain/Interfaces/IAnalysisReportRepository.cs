using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces;

/// <summary>
/// Repository interface for AnalysisReport
/// </summary>
public interface IAnalysisReportRepository : IPostgreSqlGenericRepository<AnalysisReport>
{
}
