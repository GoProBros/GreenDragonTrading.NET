using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Commands.UploadFile
{
    public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, ApiResponse<FinancialReportDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IGoogleDriveService _googleDriveService;
        private readonly ILogger<UploadFileCommandHandler> _logger;

        public UploadFileCommandHandler(
            IUnitOfWork uow,
            IGoogleDriveService googleDriveService,
            ILogger<UploadFileCommandHandler> logger)
        {
            _uow = uow;
            _googleDriveService = googleDriveService;
            _logger = logger;
        }

        public async Task<ApiResponse<FinancialReportDto>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var financialReport = await _uow.FinancialReports.GetByIdAsync(request.Id, cancellationToken);
                
                if (financialReport == null)
                {
                    return ApiResponse<FinancialReportDto>.Failure($"Không tìm thấy báo cáo tài chính với ID {request.Id}.");
                }

                // Delete old file from Google Drive if exists
                if (!string.IsNullOrEmpty(financialReport.FilePath))
                {
                    try
                    {
                        await _googleDriveService.DeleteFileAsync(financialReport.FilePath, cancellationToken);
                        _logger.LogInformation("Old file deleted from Google Drive: {FileId}", financialReport.FilePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete old file from Google Drive: {FileId}", financialReport.FilePath);
                    }
                }

                // Upload new file to Google Drive
                var fileName = $"{financialReport.Ticker}_{financialReport.Year}_Q{(int)financialReport.Period}{Path.GetExtension(request.File.FileName)}";

                var (fileId, fileUrl) = await _googleDriveService.UploadFileAsync(
                    request.File,
                    fileName,
                    DriveFolderConstants.FINANCIAL_REPORTS, // Use folder type constant
                    cancellationToken);

                // Update financial report with Google Drive file info
                financialReport.FilePath = fileId; // Store Google Drive file ID
                financialReport.FileSize = request.File.Length;
                financialReport.UpdatedAt = DateTimeOffset.UtcNow;

                _uow.FinancialReports.Update(financialReport);
                await _uow.SaveChangesAsync(cancellationToken);

                var dto = new FinancialReportDto
                {
                    Id = financialReport.Id,
                    Ticker = financialReport.Ticker,
                    Year = financialReport.Year,
                    Period = financialReport.Period,
                    FilePath = financialReport.FilePath,
                    FileUrl = fileUrl, // Use Google Drive shareable URL
                    FileSize = financialReport.FileSize,
                    ShortTermAssets = financialReport.ShortTermAssets,
                    CashAndCashEquivalents = financialReport.CashAndCashEquivalents,
                    ShortTermFinancialInvestments = financialReport.ShortTermFinancialInvestments,
                    ShortTermReceivables = financialReport.ShortTermReceivables,
                    Inventories = financialReport.Inventories,
                    LongTermAssets = financialReport.LongTermAssets,
                    LongTermReceivables = financialReport.LongTermReceivables,
                    FixedAssets = financialReport.FixedAssets,
                    TotalAssets = financialReport.TotalAssets,
                    Liabilities = financialReport.Liabilities,
                    ShortTermLiabilities = financialReport.ShortTermLiabilities,
                    LongTermLiabilities = financialReport.LongTermLiabilities,
                    OwnerEquity = financialReport.OwnerEquity,
                    TotalResources = financialReport.TotalResources,
                    Revenue = financialReport.Revenue,
                    NetRevenue = financialReport.NetRevenue,
                    CostOfGoodsSold = financialReport.CostOfGoodsSold,
                    GrossProfit = financialReport.GrossProfit,
                    NetProfit = financialReport.NetProfit,
                    ProfitBeforeTax = financialReport.ProfitBeforeTax,
                    IncomeTaxExpense = financialReport.IncomeTaxExpense,
                    ProfitAfterTax = financialReport.ProfitAfterTax,
                    CashFromOperating = financialReport.CashFromOperating,
                    CashFromInvesting = financialReport.CashFromInvesting,
                    CashFromFinancing = financialReport.CashFromFinancing,
                    NetCashFlow = financialReport.NetCashFlow,
                    BeginningCash = financialReport.BeginningCash,
                    EndingCash = financialReport.EndingCash,
                    Status = financialReport.Status,
                    CreatedAt = financialReport.CreatedAt,
                    UpdatedAt = financialReport.UpdatedAt
                };

                _logger.LogInformation("File uploaded to Google Drive for financial report: {Id}, FileId: {FileId}, URL: {FileUrl}", 
                    request.Id, fileId, fileUrl);
                return ApiResponse<FinancialReportDto>.Success(dto, "Upload file thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file for financial report {Id}", request.Id);
                throw;
            }
        }
    }
}
