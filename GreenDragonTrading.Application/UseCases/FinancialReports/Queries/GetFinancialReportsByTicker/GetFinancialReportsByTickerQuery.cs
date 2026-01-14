using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Queries.GetFinancialReportsByTicker
{
    /// <summary>
    /// Query to get paginated financial reports by ticker
    /// </summary>
    public record GetFinancialReportsByTickerQuery(
        string Ticker
    ) : PaginationQuery, IRequest<ApiResponse<PaginatedResponse<SimpleFinancialReportDto>>>;
}
