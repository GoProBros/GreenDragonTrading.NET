using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.RecalculateFinancialReportIndicators;

public class RecalculateFinancialReportIndicatorsCommandHandler(
    IUnitOfWork uow,
    IFinancialReportIndicatorCalculationService indicatorCalculationService,
    ILogger<RecalculateFinancialReportIndicatorsCommandHandler> logger)
    : IRequestHandler<RecalculateFinancialReportIndicatorsCommand, ApiResponse<RecalculateFinancialReportIndicatorsResult>>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IFinancialReportIndicatorCalculationService _indicatorCalculationService = indicatorCalculationService;
    private readonly ILogger<RecalculateFinancialReportIndicatorsCommandHandler> _logger = logger;

    public async Task<ApiResponse<RecalculateFinancialReportIndicatorsResult>> Handle(
        RecalculateFinancialReportIndicatorsCommand request,
        CancellationToken cancellationToken)
    {
        var result = new RecalculateFinancialReportIndicatorsResult();

        var candidates = await _uow.FinancialReports.GetForIndicatorRecalculationAsync(
            request.Ticker,
            request.Year,
            request.Period.HasValue ? (int)request.Period.Value : null,
            request.OnlyNullIndicatorData,
            request.MaxRecords,
            cancellationToken);

        result = result with { TotalCandidates = candidates.Count };

        foreach (var report in candidates)
        {
            try
            {
                var (comparisonYear, comparisonPeriod) = GetComparisonPeriod(report.Year, report.Period);
                var comparisonReport = await _uow.FinancialReports.GetByTickerYearPeriodAsync(
                    report.Ticker,
                    comparisonYear,
                    (int)comparisonPeriod,
                    cancellationToken);

                var recalculatedIndicatorData = _indicatorCalculationService.Calculate(
                    report.ReportData,
                    report.Period,
                    comparisonReport?.ReportData);

                if (!HasIndicatorDataChanged(report.IndicatorData, recalculatedIndicatorData))
                {
                    continue;
                }

                report.IndicatorData = recalculatedIndicatorData;
                report.UpdatedAt = recalculatedIndicatorData.CalculatedAt;
                _uow.FinancialReports.Update(report);
                result.UpdatedCount++;
            }
            catch (Exception ex)
            {
                result.FailedCount++;
                var error = $"{report.Ticker}-{report.Year}-{report.Period}: {ex.Message}";
                result.Errors.Add(error);
                _logger.LogWarning(ex, "Failed to recalculate indicator data for {Ticker}-{Year}-{Period}", report.Ticker, report.Year, report.Period);
            }
        }

        if (result.UpdatedCount > 0)
        {
            await _uow.SaveChangesAsync(cancellationToken);
        }

        return ApiResponse<RecalculateFinancialReportIndicatorsResult>.Success(
            result,
            $"Đã tính lại chỉ số: {result.UpdatedCount}/{result.TotalCandidates} bản ghi.");
    }

    private static bool HasIndicatorDataChanged(
        FinancialReportIndicatorData? current,
        FinancialReportIndicatorData? recalculated)
    {
        if (current is null || recalculated is null)
        {
            return current is not null || recalculated is not null;
        }

        return SerializeIndicatorDataWithoutCalculatedAt(current)
            != SerializeIndicatorDataWithoutCalculatedAt(recalculated);
    }

    private static string SerializeIndicatorDataWithoutCalculatedAt(FinancialReportIndicatorData data)
    {
        var comparable = new FinancialReportIndicatorData
        {
            Profitability = data.Profitability,
            LiquidityAndSolvency = data.LiquidityAndSolvency,
            Efficiency = data.Efficiency,
            Growth = data.Growth,
            BankSpecific = data.BankSpecific,
            CashFlow = data.CashFlow,
            CalculatedAt = default
        };

        return JsonSerializer.Serialize(comparable);
    }

    private static (int comparisonYear, ReportPeriod comparisonPeriod) GetComparisonPeriod(int year, ReportPeriod period)
    {
        if (period == ReportPeriod.Yearly)
        {
            return (year - 1, ReportPeriod.Yearly);
        }

        if (period == ReportPeriod.Q1)
        {
            return (year - 1, ReportPeriod.Q4);
        }

        return (year, (ReportPeriod)((int)period - 1));
    }
}
