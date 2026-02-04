using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for AnalysisReportCategory entity
/// </summary>
public class AnalysisReportCategoryRepository(GdtPostgreSqlDbContext context) : PostgreSqlGenericRepository<AnalysisReportCategory>(context), IAnalysisReportCategoryRepository
{
}
