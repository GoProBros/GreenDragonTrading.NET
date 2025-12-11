using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Symbols.Queries.GetIntradayOhlc
{
    public record GetIntradayOhlcQuery(
        string Symbol,
        string FromDate,
        string ToDate,
        bool? Ascending,
        int? Resolution
    ) :PaginationQuery, IRequest<ApiResponse<PaginatedResponse<IntradayOhlc>>>;
}
