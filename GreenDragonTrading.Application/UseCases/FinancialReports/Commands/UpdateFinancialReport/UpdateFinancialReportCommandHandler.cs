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
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<UpdateFinancialReportCommandHandler> _logger;

        public UpdateFinancialReportCommandHandler(
            IUnitOfWork uow,
            IFileStorageService fileStorageService,
            ILogger<UpdateFinancialReportCommandHandler> logger)
        {
            _uow = uow;
            _fileStorageService = fileStorageService;
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
                    FilePath = financialReport.FilePath,
                    FileUrl = financialReport.FilePath != null ? _fileStorageService.GetFileUrl(financialReport.FilePath) : null,
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
    }
}
