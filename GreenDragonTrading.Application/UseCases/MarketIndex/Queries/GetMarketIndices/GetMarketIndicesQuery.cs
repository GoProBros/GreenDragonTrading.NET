using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.MarketIndex.Queries.GetMarketIndices
{
    /// <summary>
    /// Returns a paginated, filtered list of market indices.
    /// </summary>
    /// <param name="ExchangeCode">Filter by exchange (e.g. "HSX", "HNX"). Null = all exchanges.</param>
    /// <param name="IsBenchmark">Filter to benchmark-only indices. Null = all.</param>
    /// <param name="Status">Filter by status. Null = Active only.</param>
    /// <param name="Search">Full-text search against Code and Name fields.</param>
    public record GetMarketIndicesQuery(
        string? ExchangeCode = null,
        bool? IsBenchmark = null,
        CommonStatus? Status = null,
        string? Search = null
    ) : PaginationQuery, IRequest<ApiResponse<PaginatedResponse<MarketIndexDto>>>;
}
