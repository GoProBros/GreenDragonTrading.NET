using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    public class SymbolRepository(GdtPostgreSqlDbContext context) : PostgreSqlGenericRepository<Symbol>(context), ISymbolRepository
    {
        public async Task<IEnumerable<string>> GetAllTickersAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Select(s => s.Ticker)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<Symbol> Symbols, int TotalCount)> GetSymbolsAsync(
            int pageIndex,
            int pageSize,
            SymbolType? type = null,
            string? exchange = null,
            string? sector = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsNoTracking();

            // Apply filters
            if (type.HasValue)
            {
                query = query.Where(s => s.Type == type.Value);
            }

            if (!string.IsNullOrWhiteSpace(exchange))
            {
                query = query.Where(s => s.ExchangeCode == exchange);
            }

            if (!string.IsNullOrWhiteSpace(sector))
            {
                query = query.Where(s => s.SectorId == sector);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var symbols = await query
                .OrderBy(s => s.Ticker)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (symbols, totalCount);
        }

        public async Task<(IEnumerable<Symbol> Symbols, int TotalCount)> SearchSymbolsAsync(
            string query,
            bool isTickerOnly,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var queryable = _dbSet
                .AsNoTracking()
                .Where(s => EF.Functions.ILike(s.Ticker, $"%{query}%") ||
                            (!isTickerOnly && EF.Functions.ILike(s.ViCompanyName!, $"%{query}%")) ||
                            (!isTickerOnly && EF.Functions.ILike(s.EnCompanyName!, $"%{query}%")));

            // Get total count before pagination
            var totalCount = await queryable.CountAsync(cancellationToken);

            // Apply pagination
            var symbols = await queryable
                .OrderBy(s => s.Ticker)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (symbols, totalCount);
        }

        public async Task<List<Symbol>> GetActiveSymbolsForHeatmapAsync(
            string? exchange = null,
            IList<string>? sectorIds = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(s => s.Status == CommonStatus.Active);

            // Apply exchange filter
            if (!string.IsNullOrWhiteSpace(exchange))
            {
                query = query.Where(s => s.ExchangeCode == exchange);
            }

            // Apply sector filter — sectorIds are always level-4 IDs (already expanded by caller)
            if (sectorIds != null && sectorIds.Count > 0)
            {
                query = query.Where(s => s.SectorId != null && sectorIds.Contains(s.SectorId));
            }

            // Include navigation properties after filtering
            query = query
                .Include(s => s.Exchange)
                .Include(s => s.Sector);

            // Order by ticker for consistent display
            return await query
                .OrderBy(s => s.Ticker)
                .ToListAsync(cancellationToken);
        }
    }
}
