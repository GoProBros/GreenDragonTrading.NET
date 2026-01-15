using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Queries.GetFinancialReports
{
    /// <summary>
    /// Query to get paginated financial reports with optional filters
    /// </summary>
    public record GetFinancialReportsQuery(
        string? Ticker,
        int? Year,
        ReportPeriod? Period,
        FinancialReportStatus? Status
    ) : PaginationQuery, IRequest<ApiResponse<PaginatedResponse<FinancialReportDto>>>;
}
