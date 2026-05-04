using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.MarketIndex.Queries.GetIndexConstituents
{
    /// <summary>
    /// Handles <see cref="GetIndexConstituentsQuery"/>.
    /// Returns paginated constituent symbols for the given index.
    /// </summary>
    public class GetIndexConstituentsQueryHandler(
        IUnitOfWork uow,
        ILogger<GetIndexConstituentsQueryHandler> logger)
        : IRequestHandler<GetIndexConstituentsQuery, ApiResponse<PaginatedResponse<MarketIndexSymbolDto>>>
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly ILogger<GetIndexConstituentsQueryHandler> _logger = logger;

        public async Task<ApiResponse<PaginatedResponse<MarketIndexSymbolDto>>> Handle(
            GetIndexConstituentsQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Getting constituents for index {Code} — IsActive={IsActive}, Search={Search}, Page={Page}/{Size}",
                request.IndexCode, request.IsActive, request.Search, request.PageIndex, request.PageSize);

            // Verify index exists
            var indexExists = await _uow.MarketIndices.AnyAsync(
                i => i.Code == request.IndexCode.ToUpper(), cancellationToken);

            if (!indexExists)
                throw new NotFoundException($"Market index '{request.IndexCode}' not found.");

            // ── Base query: join MarketIndexSymbol ↔ Symbol in memory ──────────
            var constituentsQuery = _uow.MarketIndexSymbols.GetQueryable()
                .Where(m => m.IndexCode == request.IndexCode.ToUpper());

            // Default: active only
            var activeFilter = request.IsActive ?? true;
            constituentsQuery = constituentsQuery.Where(m => m.IsActive == activeFilter);

            // ── Materialise so we can join with Symbol data ───────────────────
            var constituents = constituentsQuery
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.Ticker)
                .ToList();

            // Enrich with symbol metadata
            var tickers = constituents.Select(c => c.Ticker).ToList();
            var symbols = (await _uow.Symbols.FindAsync(s => tickers.Contains(s.Ticker), cancellationToken))
                .ToDictionary(s => s.Ticker);

            // ── Optional search after enrichment ─────────────────────────────
            IEnumerable<Domain.Entities.MarketIndexSymbol> filtered = constituents;
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                filtered = constituents.Where(c =>
                    c.Ticker.ToLower().Contains(search) ||
                    (symbols.TryGetValue(c.Ticker, out var s) &&
                     (s.EnCompanyName?.ToLower().Contains(search) == true ||
                      s.ViCompanyName?.ToLower().Contains(search) == true)));
            }

            var filteredList = filtered.ToList();
            var totalCount = filteredList.Count;

            var paged = filteredList
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c =>
                {
                    symbols.TryGetValue(c.Ticker, out var sym);
                    return new MarketIndexSymbolDto
                    {
                        Ticker = c.Ticker,
                        EnCompanyName = sym?.EnCompanyName,
                        ViCompanyName = sym?.ViCompanyName,
                        ExchangeCode = sym?.ExchangeCode,
                        Weight = c.Weight,
                        AddedDate = c.AddedDate,
                        DisplayOrder = c.DisplayOrder
                    };
                })
                .ToList();

            _logger.LogInformation("Returning {Count}/{Total} constituents for index {Code}.", paged.Count, totalCount, request.IndexCode);

            var paginated = PaginatedResponse<MarketIndexSymbolDto>.Create(paged, totalCount, request.PageIndex, request.PageSize);
            return ApiResponse<PaginatedResponse<MarketIndexSymbolDto>>.Success(paginated, "Lấy danh sách cổ phiếu trong chỉ số thành công");
        }
    }
}
