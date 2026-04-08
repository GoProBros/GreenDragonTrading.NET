using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Queries.GetFinancialReportIndicatorList
{
    public class GetFinancialReportIndicatorListQueryHandler(
        IUnitOfWork uow,
        IFinancialReportIndicatorCalculationService indicatorCalculationService,
        ILogger<GetFinancialReportIndicatorListQueryHandler> logger)
        : IRequestHandler<GetFinancialReportIndicatorListQuery, ApiResponse<PaginatedResponse<FinancialReportIndicatorListItemDto>>>
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly IFinancialReportIndicatorCalculationService _indicatorCalculationService = indicatorCalculationService;
        private readonly ILogger<GetFinancialReportIndicatorListQueryHandler> _logger = logger;

        public async Task<ApiResponse<PaginatedResponse<FinancialReportIndicatorListItemDto>>> Handle(
            GetFinancialReportIndicatorListQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var (reports, totalCount) = await _uow.FinancialReports.GetPaginatedAsync(
                    request.PageIndex,
                    request.PageSize,
                    request.Ticker,
                    request.Year,
                    request.Period.HasValue ? (int)request.Period.Value : null,
                    request.Status.HasValue ? (int)request.Status.Value : null,
                    cancellationToken);

                var items = new List<FinancialReportIndicatorListItemDto>(reports.Count);

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

                    items.Add(new FinancialReportIndicatorListItemDto
                    {
                        Id = report.Id,
                        Ticker = report.Ticker,
                        Year = report.Year,
                        Period = report.Period,
                        Status = report.Status,
                        IndicatorData = indicatorData,
                        CreatedAt = report.CreatedAt,
                        UpdatedAt = report.UpdatedAt
                    });
                }

                var paginated = PaginatedResponse<FinancialReportIndicatorListItemDto>.Create(
                    items,
                    totalCount,
                    request.PageIndex,
                    request.PageSize);

                return ApiResponse<PaginatedResponse<FinancialReportIndicatorListItemDto>>.Success(
                    paginated,
                    "Lấy danh sách chỉ số tài chính thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting financial report indicators with filters");
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
