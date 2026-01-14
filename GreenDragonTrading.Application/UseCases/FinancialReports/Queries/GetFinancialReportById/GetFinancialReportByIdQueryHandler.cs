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
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<GetFinancialReportByIdQueryHandler> _logger;

        public GetFinancialReportByIdQueryHandler(
            IUnitOfWork uow,
            IFileStorageService fileStorageService,
            ILogger<GetFinancialReportByIdQueryHandler> logger)
        {
            _uow = uow;
            _fileStorageService = fileStorageService;
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
                    FilePath = financialReport.FilePath,
                    FileUrl = financialReport.FilePath != null ? _fileStorageService.GetFileUrl(financialReport.FilePath) : null,
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
