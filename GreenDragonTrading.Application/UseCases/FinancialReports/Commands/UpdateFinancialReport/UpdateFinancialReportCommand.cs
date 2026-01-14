using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Commands.UpdateFinancialReport
{
    /// <summary>
    /// Command to update an existing financial report
    /// </summary>
    public record UpdateFinancialReportCommand(
        Guid Id,
        decimal? ShortTermAssets,
        decimal? CashAndCashEquivalents,
        decimal? ShortTermFinancialInvestments,
        decimal? ShortTermReceivables,
        decimal? Inventories,
        decimal? LongTermAssets,
        decimal? LongTermReceivables,
        decimal? FixedAssets,
        decimal? TotalAssets,
        decimal? Liabilities,
        decimal? ShortTermLiabilities,
        decimal? LongTermLiabilities,
        decimal? OwnerEquity,
        decimal? TotalResources,
        decimal? Revenue,
        decimal? NetRevenue,
        decimal? CostOfGoodsSold,
        decimal? GrossProfit,
        decimal? NetProfit,
        decimal? ProfitBeforeTax,
        decimal? IncomeTaxExpense,
        decimal? ProfitAfterTax,
        decimal? CashFromOperating,
        decimal? CashFromInvesting,
        decimal? CashFromFinancing,
        decimal? NetCashFlow,
        decimal? BeginningCash,
        decimal? EndingCash,
        FinancialReportStatus? Status
    ) : IRequest<ApiResponse<FinancialReportDto>>;
}
