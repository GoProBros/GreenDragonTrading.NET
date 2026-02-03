using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for AnalysisReportSource entity
/// </summary>
public class AnalysisReportSourceRepository(GdtPostgreSqlDbContext context) : PostgreSqlGenericRepository<AnalysisReportSource>(context), IAnalysisReportSourceRepository
{
}
