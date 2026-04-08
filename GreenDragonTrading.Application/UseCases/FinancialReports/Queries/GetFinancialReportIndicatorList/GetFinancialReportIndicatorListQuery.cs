using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Queries.GetFinancialReportIndicatorList
{
    /// <summary>
    /// Query to get paginated indicator snapshots with filters, aligned with financial reports list filters.
    /// </summary>
    public record GetFinancialReportIndicatorListQuery(
        string? Ticker,
        int? Year,
        ReportPeriod? Period,
        FinancialReportStatus? Status
    ) : PaginationQuery, IRequest<ApiResponse<PaginatedResponse<FinancialReportIndicatorListItemDto>>>;

    public record FinancialReportIndicatorListItemDto
    {
        public Guid Id { get; init; }
        public string Ticker { get; init; } = string.Empty;
        public int Year { get; init; }
        public ReportPeriod Period { get; init; }
        public FinancialReportStatus Status { get; init; }
        public FinancialReportIndicatorData IndicatorData { get; init; } = new();
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset UpdatedAt { get; init; }
    }
}
