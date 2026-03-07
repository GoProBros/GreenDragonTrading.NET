using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.MarketIndex.Queries.GetMarketIndices
{
    /// <summary>
    /// Handles <see cref="GetMarketIndicesQuery"/>.
    /// Applies filters and pagination in-memory using standard LINQ (no EF Core extensions).
    /// </summary>
    public class GetMarketIndicesQueryHandler(
        IUnitOfWork uow,
        ILogger<GetMarketIndicesQueryHandler> logger)
        : IRequestHandler<GetMarketIndicesQuery, ApiResponse<PaginatedResponse<MarketIndexDto>>>
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly ILogger<GetMarketIndicesQueryHandler> _logger = logger;

        public Task<ApiResponse<PaginatedResponse<MarketIndexDto>>> Handle(
            GetMarketIndicesQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Getting market indices — Exchange={Exchange}, IsBenchmark={IsBenchmark}, Status={Status}, Search={Search}, Page={Page}/{Size}",
                request.ExchangeCode, request.IsBenchmark, request.Status, request.Search, request.PageIndex, request.PageSize);

            var query = _uow.MarketIndices.GetQueryable();

            // ── Filters ────────────────────────────────────────────────────────
            var targetStatus = request.Status ?? CommonStatus.Active;
            query = query.Where(i => i.Status == targetStatus);

            if (!string.IsNullOrWhiteSpace(request.ExchangeCode))
                query = query.Where(i => i.ExchangeCode == request.ExchangeCode.ToUpper());

            if (request.IsBenchmark.HasValue)
                query = query.Where(i => i.IsBenchmark == request.IsBenchmark.Value);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(i =>
                    i.Code.ToLower().Contains(search) ||
                    i.Name.ToLower().Contains(search));
            }

            // ── Ordering ───────────────────────────────────────────────────────
            query = query.OrderBy(i => i.ExchangeCode).ThenBy(i => i.Code);

            // ── Materialise (synchronous — Application layer cannot use ToListAsync) ──
            var allItems = query.ToList();
            var totalCount = allItems.Count;

            var paged = allItems
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(i => new MarketIndexDto
                {
                    Code = i.Code,
                    Name = i.Name,
                    ExchangeCode = i.ExchangeCode,
                    Description = i.Description,
                    IsBenchmark = i.IsBenchmark,
                    Status = i.Status
                })
                .ToList();

            _logger.LogInformation("Returning {Count}/{Total} market indices.", paged.Count, totalCount);

            var paginated = PaginatedResponse<MarketIndexDto>.Create(paged, totalCount, request.PageIndex, request.PageSize);
            return Task.FromResult(ApiResponse<PaginatedResponse<MarketIndexDto>>.Success(paginated, "Lấy danh sách chỉ số thị trường thành công"));
        }
    }
}
