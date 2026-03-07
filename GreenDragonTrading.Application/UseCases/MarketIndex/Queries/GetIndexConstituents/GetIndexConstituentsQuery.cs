using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.MarketIndex.Queries.GetIndexConstituents
{
    /// <summary>
    /// Returns a paginated list of constituent symbols for a given market index.
    /// </summary>
    /// <param name="IndexCode">Target index code (e.g. "VN30").</param>
    /// <param name="IsActive">Filter by active/inactive constituents. Null = active only.</param>
    /// <param name="Search">Search by ticker or company name.</param>
    public record GetIndexConstituentsQuery(
        string IndexCode,
        bool? IsActive = null,
        string? Search = null
    ) : PaginationQuery, IRequest<ApiResponse<PaginatedResponse<MarketIndexSymbolDto>>>;
}
