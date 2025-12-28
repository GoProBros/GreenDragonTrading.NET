using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Commands.UploadFinancialReport
{
    public class UploadFinancialReportCommandHandler 
        : IRequestHandler<UploadFinancialReportCommand, ApiResponse<UploadFinancialReportResponse>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<UploadFinancialReportCommandHandler> _logger;

        public UploadFinancialReportCommandHandler(
            IUnitOfWork uow,
            IFileStorageService fileStorageService,
            ILogger<UploadFinancialReportCommandHandler> logger)
        {
            _uow = uow;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<ApiResponse<UploadFinancialReportResponse>> Handle(
            UploadFinancialReportCommand request, 
            CancellationToken cancellationToken)
        {
            try
            {
                var symbol = await _uow.Symbols.GetByIdAsync(request.Ticker, cancellationToken)
                    ?? throw new NotFoundException($"Không tìm thấy mã chứng khoán: {request.Ticker}");

                var exists = await _uow.FinancialReports.ExistsAsync(
                    request.Ticker, 
                    request.Year, 
                    (int) request.Period, 
                    cancellationToken);

                if (exists)
                {
                    throw new ConflictException(
                        $"Báo cáo tài chính {request.Ticker} - {request.Year} - Q{request.Period} đã tồn tại");
                }

                // Upload file to R2
                var folder = FileStorageConstants.GetFinancialReportPath(request.Year, request.Ticker);
                string filePath;
                long fileSize;

                using (var stream = request.File.OpenReadStream())
                {
                    (filePath, fileSize) = await _fileStorageService.UploadAsync(
                        stream,
                        request.File.FileName,
                        folder,
                        request.File.ContentType ?? "application/octet-stream",
                        cancellationToken);
                }

                _logger.LogInformation(
                    "Uploaded financial report file: {Ticker} - {Year} - Q{Period} -> {FilePath} ({FileSize} bytes)",
                    request.Ticker, request.Year, request.Period, filePath, fileSize);

                var financialReport = new FinancialReport
                {
                    Id = Guid.NewGuid(),
                    Ticker = request.Ticker,
                    Year = request.Year,
                    Period = request.Period,
                    FilePath = filePath,
                    FileSize = fileSize,
                    ContentType = request.File.ContentType,
                    Status = FinancialReportStatus.Pending,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                await _uow.FinancialReports.AddAsync(financialReport, cancellationToken);
                await _uow.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Created financial report record: {ReportId} - {Ticker}",
                    financialReport.Id, request.Ticker);

                // TODO: process file

                var reportDto = new FinancialReportDto
                {
                    Id = financialReport.Id,
                    Ticker = financialReport.Ticker,
                    Year = financialReport.Year,
                    Period = financialReport.Period,
                    FilePath = financialReport.FilePath,
                    FileUrl = _fileStorageService.GetFileUrl(filePath),
                    FileSize = financialReport.FileSize,
                    ContentType = financialReport.ContentType,
                    KeyMetrics = financialReport.KeyMetrics,
                    Status = financialReport.Status,
                    CreatedAt = financialReport.CreatedAt,
                    UpdatedAt = financialReport.UpdatedAt
                };

                var response = new UploadFinancialReportResponse
                {
                    ReportId = financialReport.Id,
                    Message = "Upload báo cáo tài chính thành công. Đang chờ xử lý.",
                    Report = reportDto
                };

                return ApiResponse<UploadFinancialReportResponse>.Success(
                    response, 
                    "Upload báo cáo tài chính thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "There is an error with uploading financial report: {Ticker} - {Year} - Q{Period}",
                    request.Ticker, request.Year, request.Period);
                throw;
            }
        }
    }
}
