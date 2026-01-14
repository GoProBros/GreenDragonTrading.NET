using GreenDragonTrading.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace GreenDragonTrading.Application.DTOs
{
    /// <summary>
    /// Financial report data transfer object
    /// </summary>
    public record FinancialReportDto
    {
        public Guid Id { get; init; }
        public string Ticker { get; init; } = null!;
        public int Year { get; init; }
        public ReportPeriod Period { get; init; }
        public string? FilePath { get; init; }
        public string? FileUrl { get; init; }
        public long? FileSize { get; init; }
        
        // Balance Sheet
        public decimal? ShortTermAssets { get; init; }
        public decimal? CashAndCashEquivalents { get; init; }
        public decimal? ShortTermFinancialInvestments { get; init; }
        public decimal? ShortTermReceivables { get; init; }
        public decimal? Inventories { get; init; }
        public decimal? LongTermAssets { get; init; }
        public decimal? LongTermReceivables { get; init; }
        public decimal? FixedAssets { get; init; }
        public decimal? TotalAssets { get; init; }
        public decimal? Liabilities { get; init; }
        public decimal? ShortTermLiabilities { get; init; }
        public decimal? LongTermLiabilities { get; init; }
        public decimal? OwnerEquity { get; init; }
        public decimal? TotalResources { get; init; }
        
        // Income Statement
        public decimal? Revenue { get; init; }
        public decimal? NetRevenue { get; init; }
        public decimal? CostOfGoodsSold { get; init; }
        public decimal? GrossProfit { get; init; }
        public decimal? NetProfit { get; init; }
        public decimal? ProfitBeforeTax { get; init; }
        public decimal? IncomeTaxExpense { get; init; }
        public decimal? ProfitAfterTax { get; init; }
        
        // Cash Flow
        public decimal? CashFromOperating { get; init; }
        public decimal? CashFromInvesting { get; init; }
        public decimal? CashFromFinancing { get; init; }
        public decimal? NetCashFlow { get; init; }
        public decimal? BeginningCash { get; init; }
        public decimal? EndingCash { get; init; }
        
        public FinancialReportStatus Status { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset UpdatedAt { get; init; }
    }

    /// <summary>
    /// Simplified financial report DTO for list views
    /// </summary>
    public record SimpleFinancialReportDto
    {
        public Guid Id { get; init; }
        public string Ticker { get; init; } = null!;
        public int Year { get; init; }
        public ReportPeriod Period { get; init; }
        public string? FileUrl { get; init; }
        public long? FileSize { get; init; }
        public decimal? Revenue { get; init; }
        public decimal? NetProfit { get; init; }
        public decimal? ProfitAfterTax { get; init; }
        public decimal? TotalAssets { get; init; }
        public FinancialReportStatus Status { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }

    /// <summary>
    /// Request model for creating financial report
    /// </summary>
    public record CreateFinancialReportRequest
    {
        public string Ticker { get; init; } = null!;
        public int Year { get; init; }
        public ReportPeriod Period { get; init; }
        
        // Financial data
        public decimal? ShortTermAssets { get; init; }
        public decimal? CashAndCashEquivalents { get; init; }
        public decimal? ShortTermFinancialInvestments { get; init; }
        public decimal? ShortTermReceivables { get; init; }
        public decimal? Inventories { get; init; }
        public decimal? LongTermAssets { get; init; }
        public decimal? LongTermReceivables { get; init; }
        public decimal? FixedAssets { get; init; }
        public decimal? TotalAssets { get; init; }
        public decimal? Liabilities { get; init; }
        public decimal? ShortTermLiabilities { get; init; }
        public decimal? LongTermLiabilities { get; init; }
        public decimal? OwnerEquity { get; init; }
        public decimal? TotalResources { get; init; }
        public decimal? Revenue { get; init; }
        public decimal? NetRevenue { get; init; }
        public decimal? CostOfGoodsSold { get; init; }
        public decimal? GrossProfit { get; init; }
        public decimal? NetProfit { get; init; }
        public decimal? ProfitBeforeTax { get; init; }
        public decimal? IncomeTaxExpense { get; init; }
        public decimal? ProfitAfterTax { get; init; }
        public decimal? CashFromOperating { get; init; }
        public decimal? CashFromInvesting { get; init; }
        public decimal? CashFromFinancing { get; init; }
        public decimal? NetCashFlow { get; init; }
        public decimal? BeginningCash { get; init; }
        public decimal? EndingCash { get; init; }
    }

    /// <summary>
    /// Request model for updating financial report
    /// </summary>
    public record UpdateFinancialReportRequest
    {
        public decimal? ShortTermAssets { get; init; }
        public decimal? CashAndCashEquivalents { get; init; }
        public decimal? ShortTermFinancialInvestments { get; init; }
        public decimal? ShortTermReceivables { get; init; }
        public decimal? Inventories { get; init; }
        public decimal? LongTermAssets { get; init; }
        public decimal? LongTermReceivables { get; init; }
        public decimal? FixedAssets { get; init; }
        public decimal? TotalAssets { get; init; }
        public decimal? Liabilities { get; init; }
        public decimal? ShortTermLiabilities { get; init; }
        public decimal? LongTermLiabilities { get; init; }
        public decimal? OwnerEquity { get; init; }
        public decimal? TotalResources { get; init; }
        public decimal? Revenue { get; init; }
        public decimal? NetRevenue { get; init; }
        public decimal? CostOfGoodsSold { get; init; }
        public decimal? GrossProfit { get; init; }
        public decimal? NetProfit { get; init; }
        public decimal? ProfitBeforeTax { get; init; }
        public decimal? IncomeTaxExpense { get; init; }
        public decimal? ProfitAfterTax { get; init; }
        public decimal? CashFromOperating { get; init; }
        public decimal? CashFromInvesting { get; init; }
        public decimal? CashFromFinancing { get; init; }
        public decimal? NetCashFlow { get; init; }
        public decimal? BeginningCash { get; init; }
        public decimal? EndingCash { get; init; }
        public FinancialReportStatus? Status { get; init; }
    }
}
