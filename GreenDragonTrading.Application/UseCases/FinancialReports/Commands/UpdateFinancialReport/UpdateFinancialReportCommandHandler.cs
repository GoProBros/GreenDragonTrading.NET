using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Commands.UpdateFinancialReport
{
    public class UpdateFinancialReportCommandHandler : IRequestHandler<UpdateFinancialReportCommand, ApiResponse<FinancialReportDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IFinancialReportIndicatorCalculationService _indicatorCalculationService;
        private readonly ILogger<UpdateFinancialReportCommandHandler> _logger;

        public UpdateFinancialReportCommandHandler(
            IUnitOfWork uow,
            IFinancialReportIndicatorCalculationService indicatorCalculationService,
            ILogger<UpdateFinancialReportCommandHandler> logger)
        {
            _uow = uow;
            _indicatorCalculationService = indicatorCalculationService;
            _logger = logger;
        }

        public async Task<ApiResponse<FinancialReportDto>> Handle(UpdateFinancialReportCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var financialReport = await _uow.FinancialReports.GetByIdAsync(request.Id, cancellationToken);
                
                if (financialReport == null)
                {
                    return ApiResponse<FinancialReportDto>.Failure($"Không tìm thấy báo cáo tài chính với ID {request.Id}.");
                }

                // Update only provided fields
                if (request.ReportData != null)
                {
                    financialReport.ReportData = request.ReportData;

                    var (comparisonYear, comparisonPeriod) = GetComparisonPeriod(financialReport.Year, financialReport.Period);
                    var comparisonReport = await _uow.FinancialReports.GetByTickerYearPeriodAsync(
                        financialReport.Ticker,
                        comparisonYear,
                        (int)comparisonPeriod,
                        cancellationToken);

                    financialReport.IndicatorData = _indicatorCalculationService.Calculate(
                        request.ReportData,
                        financialReport.Period,
                        comparisonReport?.ReportData);
                }
                
                if (request.Status.HasValue)
                {
                    financialReport.Status = request.Status.Value;
                }

                financialReport.UpdatedAt = DateTimeOffset.UtcNow;

                _uow.FinancialReports.Update(financialReport);
                await _uow.SaveChangesAsync(cancellationToken);

                var dto = new FinancialReportDto
                {
                    Id = financialReport.Id,
                    Ticker = financialReport.Ticker,
                    Year = financialReport.Year,
                    Period = financialReport.Period,
                    ReportData = financialReport.ReportData,
                    IndicatorData = financialReport.IndicatorData,
                    FilePath = financialReport.FilePath,
                    FileUrl = financialReport.FilePath ?? null,
                    FileSize = financialReport.FileSize,
                    Status = financialReport.Status,
                    CreatedAt = financialReport.CreatedAt,
                    UpdatedAt = financialReport.UpdatedAt
                };

                _logger.LogInformation("Financial report updated: {Id}", request.Id);
                return ApiResponse<FinancialReportDto>.Success(dto, "Cập nhật báo cáo tài chính thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating financial report {Id}", request.Id);
                throw;
            }
        }

        private static (int comparisonYear, Domain.Enums.ReportPeriod comparisonPeriod) GetComparisonPeriod(
            int year,
            Domain.Enums.ReportPeriod period)
        {
            if (period == Domain.Enums.ReportPeriod.Yearly)
            {
                return (year - 1, Domain.Enums.ReportPeriod.Yearly);
            }

            if (period == Domain.Enums.ReportPeriod.Q1)
            {
                return (year - 1, Domain.Enums.ReportPeriod.Q4);
            }

            return (year, (Domain.Enums.ReportPeriod)((int)period - 1));
        }
    }
}
