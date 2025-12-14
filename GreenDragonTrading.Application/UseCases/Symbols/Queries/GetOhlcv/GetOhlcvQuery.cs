using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Symbols.Queries.GetOhlcv
{
    /// <summary>
    /// Query to get stock chart data from Finsc API
    /// </summary>
    /// <param name="Symbol"></param>
    /// <param name="Resolution"></param>
    /// <param name="FromDate"></param>
    /// <param name="ToDate"></param>
    public record GetOhlcvQuery(
        string Symbol,
        string? Resolution,
        string? FromDate,
        string? ToDate
        ) : IRequest<ApiResponse<FinscStockResponse>>;
}
