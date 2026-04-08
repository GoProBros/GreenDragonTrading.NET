using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    public class FinancialReportRepository(GdtPostgreSqlDbContext context) : PostgreSqlGenericRepository<FinancialReport>(context), IFinancialReportRepository
    {
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

        public async Task<(List<FinancialReport>, int)> GetPaginatedAsync(
            int pageIndex, 
            int pageSize, 
            string? ticker = null, 
            int? year = null, 
            int? period = null, 
            int? status = null, 
            CancellationToken cancellationToken = default)
        {
            var query = _context.Set<FinancialReport>()
                .Include(f => f.Symbol)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(ticker))
            {
                query = query.Where(f => f.Ticker == ticker);
            }

            if (year.HasValue)
            {
                query = query.Where(f => f.Year == year.Value);
            }

            if (period.HasValue)
            {
                query = query.Where(f => f.Period == (ReportPeriod)period.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(f => f.Status == (FinancialReportStatus)status.Value);
            }

            // Order by year and period descending
            query = query
                .OrderByDescending(f => f.Year)
                .ThenByDescending(f => f.Period);

            var totalCount = await query.CountAsync(cancellationToken);
            
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<List<FinancialReport>> GetForIndicatorRecalculationAsync(
            string? ticker = null,
            int? year = null,
            int? period = null,
            bool onlyNullIndicatorData = true,
            int maxRecords = 2000,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Set<FinancialReport>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(ticker))
            {
                query = query.Where(f => f.Ticker == ticker);
            }

            if (year.HasValue)
            {
                query = query.Where(f => f.Year == year.Value);
            }

            if (period.HasValue)
            {
                query = query.Where(f => f.Period == (ReportPeriod)period.Value);
            }

            if (onlyNullIndicatorData)
            {
                query = query.Where(f => f.IndicatorData == null);
            }

            return await query
                .OrderBy(f => f.Ticker)
                .ThenBy(f => f.Year)
                .ThenBy(f => f.Period)
                .Take(maxRecords)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<FinancialReport>> GetRecentQuarterlyByTickerAsync(
            string ticker,
            int count,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<FinancialReport>()
                .Where(f => f.Ticker == ticker && f.Period != ReportPeriod.Yearly)
                .OrderByDescending(f => f.Year)
                .ThenByDescending(f => f.Period)
                .Take(count)
                .ToListAsync(cancellationToken);
        }
    }
}
