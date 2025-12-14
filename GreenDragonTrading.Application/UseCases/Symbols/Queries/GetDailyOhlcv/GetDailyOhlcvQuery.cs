using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Symbols.Queries.GetDailyOhlcv
{
    /// <summary>
    /// Query to get daily OHLCV chart data from Finsc API
    /// </summary>
    /// <param name="Symbol">Stock ticker (e.g., FPT, VNM, SSI)</param>
    /// <param name="FromDate">Start date in dd/MM/yyyy format (default: 3 months ago)</param>
    /// <param name="ToDate">End date in dd/MM/yyyy format (default: today)</param>
    public record GetDailyOhlcvQuery(
        string Symbol,
        string? FromDate,
        string? ToDate
    ) : IRequest<ApiResponse<FinscStockResponse>>;
}
