using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Queries.GetRecentQuarterIndicators
{
    /// <summary>
    /// Query to get recent quarterly indicator snapshots by ticker.
    /// </summary>
    public record GetRecentQuarterIndicatorsQuery(string Ticker, int Count = 5)
        : IRequest<ApiResponse<IReadOnlyCollection<QuarterIndicatorItemDto>>>;

    public record QuarterIndicatorItemDto
    {
        public Guid ReportId { get; init; }
        public int Year { get; init; }
        public ReportPeriod Period { get; init; }
        public FinancialReportIndicatorData IndicatorData { get; init; } = new();
    }
}
