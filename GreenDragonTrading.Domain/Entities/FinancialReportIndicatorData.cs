using System.Text.Json.Serialization;

namespace GreenDragonTrading.Domain.Entities
{
    /// <summary>
    /// Calculated indicator snapshot for a financial report.
    /// </summary>
    public class FinancialReportIndicatorData
    {
        [JsonPropertyName("profitability")]
        public ProfitabilityRatiosDto Profitability { get; set; } = new();

        [JsonPropertyName("liquidityAndSolvency")]
        public LiquidityAndSolvencyRatiosDto LiquidityAndSolvency { get; set; } = new();

        [JsonPropertyName("efficiency")]
        public EfficiencyRatiosDto Efficiency { get; set; } = new();

        [JsonPropertyName("growth")]
        public GrowthRatiosDto Growth { get; set; } = new();

        [JsonPropertyName("bankSpecific")]
        public BankSpecificRatiosDto BankSpecific { get; set; } = new();

        [JsonPropertyName("cashFlow")]
        public CashFlowRatiosDto CashFlow { get; set; } = new();

        [JsonPropertyName("calculatedAt")]
        public DateTimeOffset CalculatedAt { get; set; } = DateTimeOffset.UtcNow;
    }

    public class ProfitabilityRatiosDto
    {
        [JsonPropertyName("grossMargin")]
        public decimal? GrossMargin { get; set; }

        [JsonPropertyName("operatingProfitMargin")]
        public decimal? OperatingProfitMargin { get; set; }

        [JsonPropertyName("netMargin")]
        public decimal? NetMargin { get; set; }

        [JsonPropertyName("roe")]
        public decimal? Roe { get; set; }

        [JsonPropertyName("roa")]
        public decimal? Roa { get; set; }

        [JsonPropertyName("returnOnFixedAssets")]
        public decimal? ReturnOnFixedAssets { get; set; }
    }

    public class LiquidityAndSolvencyRatiosDto
    {
        [JsonPropertyName("currentRatio")]
        public decimal? CurrentRatio { get; set; }

        [JsonPropertyName("quickRatio")]
        public decimal? QuickRatio { get; set; }

        [JsonPropertyName("cashRatio")]
        public decimal? CashRatio { get; set; }

        [JsonPropertyName("debtToEquity")]
        public decimal? DebtToEquity { get; set; }

        [JsonPropertyName("debtRatio")]
        public decimal? DebtRatio { get; set; }

        [JsonPropertyName("longTermDebtRatio")]
        public decimal? LongTermDebtRatio { get; set; }

        [JsonPropertyName("interestCoverageRatio")]
        public decimal? InterestCoverageRatio { get; set; }

        [JsonPropertyName("retainedEarningsToTotalAssets")]
        public decimal? RetainedEarningsToTotalAssets { get; set; }
    }

    public class EfficiencyRatiosDto
    {
        [JsonPropertyName("totalAssetTurnover")]
        public decimal? TotalAssetTurnover { get; set; }

        [JsonPropertyName("inventoryTurnover")]
        public decimal? InventoryTurnover { get; set; }
    }

    public class GrowthRatiosDto
    {
        [JsonPropertyName("comparisonType")]
        public string? ComparisonType { get; set; }

        [JsonPropertyName("grossProfitGrowth")]
        public decimal? GrossProfitGrowth { get; set; }

        [JsonPropertyName("revenueGrowth")]
        public decimal? RevenueGrowth { get; set; }
    }

    public class BankSpecificRatiosDto
    {
        [JsonPropertyName("nim")]
        public decimal? Nim { get; set; }

        [JsonPropertyName("nonInterestIncomeRatio")]
        public decimal? NonInterestIncomeRatio { get; set; }
    }

    public class CashFlowRatiosDto
    {
        [JsonPropertyName("operatingCashFlowToNetProfit")]
        public decimal? OperatingCashFlowToNetProfit { get; set; }
    }
}
