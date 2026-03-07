using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.MarketIndex.Queries.GetMarketIndexById
{
    /// <summary>
    /// Returns the details of a single market index by its code.
    /// </summary>
    /// <param name="Code">Index code (e.g. "VN30").</param>
    public record GetMarketIndexByIdQuery(string Code)
        : IRequest<ApiResponse<MarketIndexDto>>;
}
