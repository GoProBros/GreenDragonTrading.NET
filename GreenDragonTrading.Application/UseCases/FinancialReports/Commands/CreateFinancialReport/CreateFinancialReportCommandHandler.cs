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
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<CreateFinancialReportCommandHandler> _logger;

        public CreateFinancialReportCommandHandler(
            IUnitOfWork uow,
            IFileStorageService fileStorageService,
            ILogger<CreateFinancialReportCommandHandler> logger)
        {
            _uow = uow;
            _fileStorageService = fileStorageService;
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

                string? filePath = null;
                long? fileSize = null;

                // Create financial report entity
                var financialReport = new FinancialReport
                {
                    Id = Guid.NewGuid(),
                    Ticker = request.Ticker,
                    Year = request.Year,
                    Period = request.Period,
                    FilePath = filePath,
                    FileSize = fileSize,
                    
                    // Balance Sheet
                    ShortTermAssets = request.ShortTermAssets,
                    CashAndCashEquivalents = request.CashAndCashEquivalents,
                    ShortTermFinancialInvestments = request.ShortTermFinancialInvestments,
                    ShortTermReceivables = request.ShortTermReceivables,
                    Inventories = request.Inventories,
                    LongTermAssets = request.LongTermAssets,
                    LongTermReceivables = request.LongTermReceivables,
                    FixedAssets = request.FixedAssets,
                    TotalAssets = request.TotalAssets,
                    Liabilities = request.Liabilities,
                    ShortTermLiabilities = request.ShortTermLiabilities,
                    LongTermLiabilities = request.LongTermLiabilities,
                    OwnerEquity = request.OwnerEquity,
                    TotalResources = request.TotalResources,
                    
                    // Income Statement
                    Revenue = request.Revenue,
                    NetRevenue = request.NetRevenue,
                    CostOfGoodsSold = request.CostOfGoodsSold,
                    GrossProfit = request.GrossProfit,
                    NetProfit = request.NetProfit,
                    ProfitBeforeTax = request.ProfitBeforeTax,
                    IncomeTaxExpense = request.IncomeTaxExpense,
                    ProfitAfterTax = request.ProfitAfterTax,
                    
                    // Cash Flow
                    CashFromOperating = request.CashFromOperating,
                    CashFromInvesting = request.CashFromInvesting,
                    CashFromFinancing = request.CashFromFinancing,
                    NetCashFlow = request.NetCashFlow,
                    BeginningCash = request.BeginningCash,
                    EndingCash = request.EndingCash,
                    
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
                FilePath = report.FilePath,
                FileUrl = report.FilePath != null ? _fileStorageService.GetFileUrl(report.FilePath) : null,
                FileSize = report.FileSize,
                
                // Balance Sheet
                ShortTermAssets = report.ShortTermAssets,
                CashAndCashEquivalents = report.CashAndCashEquivalents,
                ShortTermFinancialInvestments = report.ShortTermFinancialInvestments,
                ShortTermReceivables = report.ShortTermReceivables,
                Inventories = report.Inventories,
                LongTermAssets = report.LongTermAssets,
                LongTermReceivables = report.LongTermReceivables,
                FixedAssets = report.FixedAssets,
                TotalAssets = report.TotalAssets,
                Liabilities = report.Liabilities,
                ShortTermLiabilities = report.ShortTermLiabilities,
                LongTermLiabilities = report.LongTermLiabilities,
                OwnerEquity = report.OwnerEquity,
                TotalResources = report.TotalResources,
                
                // Income Statement
                Revenue = report.Revenue,
                NetRevenue = report.NetRevenue,
                CostOfGoodsSold = report.CostOfGoodsSold,
                GrossProfit = report.GrossProfit,
                NetProfit = report.NetProfit,
                ProfitBeforeTax = report.ProfitBeforeTax,
                IncomeTaxExpense = report.IncomeTaxExpense,
                ProfitAfterTax = report.ProfitAfterTax,
                
                // Cash Flow
                CashFromOperating = report.CashFromOperating,
                CashFromInvesting = report.CashFromInvesting,
                CashFromFinancing = report.CashFromFinancing,
                NetCashFlow = report.NetCashFlow,
                BeginningCash = report.BeginningCash,
                EndingCash = report.EndingCash,
                
                Status = report.Status,
                CreatedAt = report.CreatedAt,
                UpdatedAt = report.UpdatedAt
            };
        }
    }
}
