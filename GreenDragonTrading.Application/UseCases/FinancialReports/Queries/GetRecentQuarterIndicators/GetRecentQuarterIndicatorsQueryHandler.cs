using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Queries.GetRecentQuarterIndicators
{
    public class GetRecentQuarterIndicatorsQueryHandler(
        IUnitOfWork uow,
        IFinancialReportIndicatorCalculationService indicatorCalculationService,
        ILogger<GetRecentQuarterIndicatorsQueryHandler> logger)
        : IRequestHandler<GetRecentQuarterIndicatorsQuery, ApiResponse<IReadOnlyCollection<QuarterIndicatorItemDto>>>
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly IFinancialReportIndicatorCalculationService _indicatorCalculationService = indicatorCalculationService;
        private readonly ILogger<GetRecentQuarterIndicatorsQueryHandler> _logger = logger;

        public async Task<ApiResponse<IReadOnlyCollection<QuarterIndicatorItemDto>>> Handle(
            GetRecentQuarterIndicatorsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var ticker = request.Ticker.Trim().ToUpperInvariant();
                var reports = await _uow.FinancialReports.GetRecentQuarterlyByTickerAsync(
                    ticker,
                    request.Count,
                    cancellationToken);

                if (reports.Count == 0)
                {
                    return ApiResponse<IReadOnlyCollection<QuarterIndicatorItemDto>>.Failure(
                        $"Không tìm thấy dữ liệu quý cho mã {ticker}.");
                }

                var result = new List<QuarterIndicatorItemDto>();

                foreach (var report in reports)
                {
                    var indicatorData = report.IndicatorData;

                    if (indicatorData == null)
                    {
                        var (comparisonYear, comparisonPeriod) = GetComparisonPeriod(report.Year, report.Period);
                        var comparisonReport = await _uow.FinancialReports.GetByTickerYearPeriodAsync(
                            report.Ticker,
                            comparisonYear,
                            (int)comparisonPeriod,
                            cancellationToken);

                        indicatorData = _indicatorCalculationService.Calculate(
                            report.ReportData,
                            report.Period,
                            comparisonReport?.ReportData);
                    }

                    result.Add(new QuarterIndicatorItemDto
                    {
                        ReportId = report.Id,
                        Year = report.Year,
                        Period = report.Period,
                        IndicatorData = indicatorData
                    });
                }

                return ApiResponse<IReadOnlyCollection<QuarterIndicatorItemDto>>.Success(
                    result,
                    $"Lấy {result.Count} quý gần nhất cho {ticker} thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent quarter indicators for {Ticker}", request.Ticker);
                throw;
            }
        }

        private static (int comparisonYear, ReportPeriod comparisonPeriod) GetComparisonPeriod(int year, ReportPeriod period)
        {
            if (period == ReportPeriod.Q1)
            {
                return (year - 1, ReportPeriod.Q4);
            }

            return (year, (ReportPeriod)((int)period - 1));
        }
    }
}
