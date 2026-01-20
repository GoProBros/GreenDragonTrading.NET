using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Queries.GetFinancialReportById
{
    public class GetFinancialReportByIdQueryHandler : IRequestHandler<GetFinancialReportByIdQuery, ApiResponse<FinancialReportDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<GetFinancialReportByIdQueryHandler> _logger;

        public GetFinancialReportByIdQueryHandler(
            IUnitOfWork uow,
            ILogger<GetFinancialReportByIdQueryHandler> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<ApiResponse<FinancialReportDto>> Handle(GetFinancialReportByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var financialReport = await _uow.FinancialReports.GetByIdAsync(request.Id, cancellationToken);
                
                if (financialReport == null)
                {
                    return ApiResponse<FinancialReportDto>.Failure($"Không tìm thấy báo cáo tài chính với ID {request.Id}.");
                }

                var dto = new FinancialReportDto
                {
                    Id = financialReport.Id,
                    Ticker = financialReport.Ticker,
                    Year = financialReport.Year,
                    Period = financialReport.Period,
                    ReportData = financialReport.ReportData,
                    FilePath = financialReport.FilePath,
                    FileUrl = financialReport.FilePath,
                    FileSize = financialReport.FileSize,
                    Status = financialReport.Status,
                    CreatedAt = financialReport.CreatedAt,
                    UpdatedAt = financialReport.UpdatedAt
                };

                return ApiResponse<FinancialReportDto>.Success(dto, "Lấy báo cáo tài chính thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting financial report by ID {Id}", request.Id);
                throw;
            }
        }
    }
}
