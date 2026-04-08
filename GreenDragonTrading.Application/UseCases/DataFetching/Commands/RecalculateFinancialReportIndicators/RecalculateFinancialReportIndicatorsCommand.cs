using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.RecalculateFinancialReportIndicators;

/// <summary>
/// Recalculates financial indicators for a batch of financial reports.
/// </summary>
public record RecalculateFinancialReportIndicatorsCommand(
    string? Ticker,
    int? Year,
    ReportPeriod? Period,
    bool OnlyNullIndicatorData = true,
    int MaxRecords = 2000
) : IRequest<ApiResponse<RecalculateFinancialReportIndicatorsResult>>;

public record RecalculateFinancialReportIndicatorsResult
{
    public int TotalCandidates { get; init; }
    public int UpdatedCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> Errors { get; init; } = new();
};
