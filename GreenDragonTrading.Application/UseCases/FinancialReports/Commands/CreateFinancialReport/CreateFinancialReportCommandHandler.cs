using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Commands.CreateFinancialReport
{
    public class CreateFinancialReportCommandHandler : IRequestHandler<CreateFinancialReportCommand, ApiResponse<FinancialReportDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IFinancialReportIndicatorCalculationService _indicatorCalculationService;
        private readonly ILogger<CreateFinancialReportCommandHandler> _logger;

        public CreateFinancialReportCommandHandler(
            IUnitOfWork uow,
            IFinancialReportIndicatorCalculationService indicatorCalculationService,
            ILogger<CreateFinancialReportCommandHandler> logger)
        {
            _uow = uow;
            _indicatorCalculationService = indicatorCalculationService;
            _logger = logger;
        }

        public async Task<ApiResponse<FinancialReportDto>> Handle(CreateFinancialReportCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if symbol exists
                var symbol = await _uow.Symbols.GetByIdAsync(request.Ticker, cancellationToken);
                if (symbol == null)
                {
                    return ApiResponse<FinancialReportDto>.Failure($"Không tìm thấy mã chứng khoán {request.Ticker}.");
                }

                // Check if report already exists
                var exists = await _uow.FinancialReports.ExistsAsync(
                    request.Ticker,
                    request.Year,
                    (int)request.Period,
                    cancellationToken);

                if (exists)
                {
                    return ApiResponse<FinancialReportDto>.Failure(
                        $"Báo cáo tài chính cho {request.Ticker} - {request.Year} - {request.Period} đã tồn tại.");
                }

                // Create financial report entity
                var (comparisonYear, comparisonPeriod) = GetComparisonPeriod(request.Year, request.Period);
                var comparisonReport = await _uow.FinancialReports.GetByTickerYearPeriodAsync(
                    request.Ticker,
                    comparisonYear,
                    (int)comparisonPeriod,
                    cancellationToken);

                var indicatorData = _indicatorCalculationService.Calculate(
                    request.ReportData,
                    request.Period,
                    comparisonReport?.ReportData);

                var financialReport = new FinancialReport
                {
                    Id = Guid.NewGuid(),
                    Ticker = request.Ticker,
                    Year = request.Year,
                    Period = request.Period,
                    ReportData = request.ReportData,
                    IndicatorData = indicatorData,
                    Status = FinancialReportStatus.Completed,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                await _uow.FinancialReports.AddAsync(financialReport, cancellationToken);
                await _uow.SaveChangesAsync(cancellationToken);

                var dto = MapToDto(financialReport);

                _logger.LogInformation("Financial report created: {Id} for {Ticker}", financialReport.Id, request.Ticker);
                return ApiResponse<FinancialReportDto>.Success(dto, "Tạo báo cáo tài chính thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating financial report for {Ticker}", request.Ticker);
                throw;
            }
        }

        private FinancialReportDto MapToDto(FinancialReport report)
        {
            return new FinancialReportDto
            {
                Id = report.Id,
                Ticker = report.Ticker,
                Year = report.Year,
                Period = report.Period,
                ReportData = report.ReportData,
                IndicatorData = report.IndicatorData,
                FilePath = report.FilePath,
                FileUrl = report.FilePath,
                FileSize = report.FileSize,
                Status = report.Status,
                CreatedAt = report.CreatedAt,
                UpdatedAt = report.UpdatedAt
            };
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
