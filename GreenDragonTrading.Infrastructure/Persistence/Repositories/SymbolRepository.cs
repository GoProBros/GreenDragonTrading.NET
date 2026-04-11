using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

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
            var trimmedQuery = query?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(trimmedQuery))
            {
                var allQuery = _dbSet.AsNoTracking();
                var totalAll = await allQuery.CountAsync(cancellationToken);
                var allSymbols = await allQuery
                    .OrderBy(s => s.Ticker)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                return (allSymbols, totalAll);
            }

            if (isTickerOnly)
            {
                var tickerQueryable = _dbSet
                    .AsNoTracking()
                    .Where(s => EF.Functions.ILike(s.Ticker, $"%{trimmedQuery}%"));

                var tickerTotalCount = await tickerQueryable.CountAsync(cancellationToken);
                var tickerSymbols = await tickerQueryable
                    .OrderBy(s => s.Ticker)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                return (tickerSymbols, tickerTotalCount);
            }

            var normalizedQuery = NormalizeSearchText(trimmedQuery);

            // Symbol count is typically small enough for in-memory normalized matching.
            var allSymbolsForSearch = await _dbSet
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var matchedSymbols = allSymbolsForSearch
                .Where(s =>
                    ContainsNormalized(s.Ticker, normalizedQuery) ||
                    ContainsNormalized(s.ViCompanyName, normalizedQuery) ||
                    ContainsNormalized(s.EnCompanyName, normalizedQuery));

            var totalCount = matchedSymbols.Count();
            var symbols = matchedSymbols
                .OrderBy(s => s.Ticker)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (symbols, totalCount);
        }

        private static bool ContainsNormalized(string? source, string normalizedQuery)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(normalizedQuery))
            {
                return false;
            }

            var normalizedSource = NormalizeSearchText(source);
            return normalizedSource.Contains(normalizedQuery, StringComparison.Ordinal);
        }

        private static string NormalizeSearchText(string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);
            var previousWasSpace = false;

            foreach (var ch in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (category == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsLetterOrDigit(ch))
                {
                    var lowered = char.ToLowerInvariant(ch);
                    builder.Append(lowered == 'đ' ? 'd' : lowered);
                    previousWasSpace = false;
                    continue;
                }

                if (!previousWasSpace)
                {
                    builder.Append(' ');
                    previousWasSpace = true;
                }
            }

            return builder.ToString().Trim();
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
