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
                if (request.ShortTermAssets.HasValue) financialReport.ShortTermAssets = request.ShortTermAssets.Value;
                if (request.CashAndCashEquivalents.HasValue) financialReport.CashAndCashEquivalents = request.CashAndCashEquivalents.Value;
                if (request.ShortTermFinancialInvestments.HasValue) financialReport.ShortTermFinancialInvestments = request.ShortTermFinancialInvestments.Value;
                if (request.ShortTermReceivables.HasValue) financialReport.ShortTermReceivables = request.ShortTermReceivables.Value;
                if (request.Inventories.HasValue) financialReport.Inventories = request.Inventories.Value;
                if (request.LongTermAssets.HasValue) financialReport.LongTermAssets = request.LongTermAssets.Value;
                if (request.LongTermReceivables.HasValue) financialReport.LongTermReceivables = request.LongTermReceivables.Value;
                if (request.FixedAssets.HasValue) financialReport.FixedAssets = request.FixedAssets.Value;
                if (request.TotalAssets.HasValue) financialReport.TotalAssets = request.TotalAssets.Value;
                if (request.Liabilities.HasValue) financialReport.Liabilities = request.Liabilities.Value;
                if (request.ShortTermLiabilities.HasValue) financialReport.ShortTermLiabilities = request.ShortTermLiabilities.Value;
                if (request.LongTermLiabilities.HasValue) financialReport.LongTermLiabilities = request.LongTermLiabilities.Value;
                if (request.OwnerEquity.HasValue) financialReport.OwnerEquity = request.OwnerEquity.Value;
                if (request.TotalResources.HasValue) financialReport.TotalResources = request.TotalResources.Value;
                if (request.Revenue.HasValue) financialReport.Revenue = request.Revenue.Value;
                if (request.NetRevenue.HasValue) financialReport.NetRevenue = request.NetRevenue.Value;
                if (request.CostOfGoodsSold.HasValue) financialReport.CostOfGoodsSold = request.CostOfGoodsSold.Value;
                if (request.GrossProfit.HasValue) financialReport.GrossProfit = request.GrossProfit.Value;
                if (request.NetProfit.HasValue) financialReport.NetProfit = request.NetProfit.Value;
                if (request.ProfitBeforeTax.HasValue) financialReport.ProfitBeforeTax = request.ProfitBeforeTax.Value;
                if (request.IncomeTaxExpense.HasValue) financialReport.IncomeTaxExpense = request.IncomeTaxExpense.Value;
                if (request.ProfitAfterTax.HasValue) financialReport.ProfitAfterTax = request.ProfitAfterTax.Value;
                if (request.CashFromOperating.HasValue) financialReport.CashFromOperating = request.CashFromOperating.Value;
                if (request.CashFromInvesting.HasValue) financialReport.CashFromInvesting = request.CashFromInvesting.Value;
                if (request.CashFromFinancing.HasValue) financialReport.CashFromFinancing = request.CashFromFinancing.Value;
                if (request.NetCashFlow.HasValue) financialReport.NetCashFlow = request.NetCashFlow.Value;
                if (request.BeginningCash.HasValue) financialReport.BeginningCash = request.BeginningCash.Value;
                if (request.EndingCash.HasValue) financialReport.EndingCash = request.EndingCash.Value;
                if (request.Status.HasValue) financialReport.Status = request.Status.Value;

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
