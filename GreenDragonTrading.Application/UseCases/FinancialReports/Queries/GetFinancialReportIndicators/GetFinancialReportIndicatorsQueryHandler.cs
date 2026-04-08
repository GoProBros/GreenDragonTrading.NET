using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Queries.GetFinancialReportIndicators
{
    public class GetFinancialReportIndicatorsQueryHandler(
        IUnitOfWork uow,
        IFinancialReportIndicatorCalculationService indicatorCalculationService,
        ILogger<GetFinancialReportIndicatorsQueryHandler> logger)
        : IRequestHandler<GetFinancialReportIndicatorsQuery, ApiResponse<FinancialReportIndicatorData>>
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly IFinancialReportIndicatorCalculationService _indicatorCalculationService = indicatorCalculationService;
        private readonly ILogger<GetFinancialReportIndicatorsQueryHandler> _logger = logger;

        public async Task<ApiResponse<FinancialReportIndicatorData>> Handle(
            GetFinancialReportIndicatorsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var report = await _uow.FinancialReports.GetByIdAsync(request.Id, cancellationToken);
                if (report == null)
                {
                    return ApiResponse<FinancialReportIndicatorData>.Failure($"Không tìm thấy báo cáo tài chính với ID {request.Id}.");
                }

                if (report.IndicatorData != null)
                {
                    return ApiResponse<FinancialReportIndicatorData>.Success(report.IndicatorData, "Lấy chỉ số tài chính thành công.");
                }

                var (comparisonYear, comparisonPeriod) = GetComparisonPeriod(report.Year, report.Period);
                var comparisonReport = await _uow.FinancialReports.GetByTickerYearPeriodAsync(
                    report.Ticker,
                    comparisonYear,
                    (int)comparisonPeriod,
                    cancellationToken);

                var calculatedIndicatorData = _indicatorCalculationService.Calculate(
                    report.ReportData,
                    report.Period,
                    comparisonReport?.ReportData);

                return ApiResponse<FinancialReportIndicatorData>.Success(
                    calculatedIndicatorData,
                    "Lấy chỉ số tài chính thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting indicator data for financial report {Id}", request.Id);
                throw;
            }
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
}
