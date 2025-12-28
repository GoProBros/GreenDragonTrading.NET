using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using GreenDragonTrading.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    public class FinancialReportRepository : PostgreSqlGenericRepository<FinancialReport>, IFinancialReportRepository
    {
        public FinancialReportRepository(GdtPostgreSqlDbContext context) : base(context)
        {
        }

        public async Task<FinancialReport?> GetByTickerYearPeriodAsync(
            string ticker, 
            int year, 
            int period, 
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<FinancialReport>()
                .Include(f => f.Symbol)
                .FirstOrDefaultAsync(
                    f => f.Ticker == ticker && 
                         f.Year == year && 
                         f.Period == (ReportPeriod)period,
                    cancellationToken);
        }

        public async Task<bool> ExistsAsync(
            string ticker, 
            int year, 
            int period, 
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<FinancialReport>()
                .AnyAsync(
                    f => f.Ticker == ticker && 
                         f.Year == year && 
                         f.Period == (ReportPeriod)period,
                    cancellationToken);
        }

        public async Task<(IEnumerable<FinancialReport>, int)> GetByTickerPaginatedAsync(
            string ticker, 
            int pageIndex, 
            int pageSize, 
            CancellationToken cancellationToken = default)
        {
            var query = _context.Set<FinancialReport>()
                .Include(f => f.Symbol)
                .Where(f => f.Ticker == ticker)
                .OrderByDescending(f => f.Year)
                .ThenByDescending(f => f.Period);

            var totalCount = await query.CountAsync(cancellationToken);
            
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
