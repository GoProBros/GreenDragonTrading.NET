using System.Text.Json.Serialization;

namespace GreenDragonTrading.Domain.Entities
{
    /// <summary>
    /// Main DTO containing all financial report data
    /// </summary>
    public class FinancialReportData
    {
        /// <summary>
        /// Bảng cân đối kế toán - Balance Sheet
        /// </summary>
        [JsonPropertyName("balanceSheet")]
        public BalanceSheetDto? BalanceSheet { get; set; }

        /// <summary>
        /// Báo cáo kết quả kinh doanh - Income Statement
        /// </summary>
        [JsonPropertyName("incomeStatement")]
        public IncomeStatementDto? IncomeStatement { get; set; }

        /// <summary>
        /// Báo cáo lưu chuyển tiền tệ - Cash Flow Statement
        /// </summary>
        [JsonPropertyName("cashFlowStatement")]
        public CashFlowDto? CashFlowStatement { get; set; }
    }

    public class BalanceSheetDto
    {
        /// <summary>
        /// 1. Tài sản ngắn hạn - SHORT_TERM_ASSETS
        /// </summary>
        [JsonPropertyName("shortTermAssets")]
        public ShortTermAssetsDto? ShortTermAssets { get; set; }

        /// <summary>
        /// 2. Tài sản dài hạn - LONG_TERM_ASSETS
        /// </summary>
        [JsonPropertyName("longTermAssets")]
        public LongTermAssetsDto? LongTermAssets { get; set; }

        /// <summary>
        /// 3. Tổng tài sản (Ngân hàng) - TOTAL_ASSETS (Bank)
        /// </summary>
        [JsonPropertyName("bankAssets")]
        public BankAssetsDto? BankAssets { get; set; }

        /// <summary>
        /// 4. Tài sản tài chính ngắn hạn (Chứng khoán) - SHORT_TERM_FINANCIAL_ASSETS
        /// </summary>
        [JsonPropertyName("shortTermFinancialAssets")]
        public ShortTermFinancialAssetsDto? ShortTermFinancialAssets { get; set; }

        /// <summary>
        /// 5. Tài sản tự doanh và nguồn vốn - TRADING_AND_CAPITAL_ASSETS
        /// </summary>
        [JsonPropertyName("tradingAndCapitalAssets")]
        public TradingAndCapitalAssetsDto? TradingAndCapitalAssets { get; set; }

        /// <summary>
        /// 6. Nợ phải trả - ACCOUNTS_PAYABLE
        /// </summary>
        [JsonPropertyName("liabilities")]
        public LiabilitiesDto? Liabilities { get; set; }

        /// <summary>
        /// 7. Nợ vay - BORROWINGS
        /// </summary>
        [JsonPropertyName("borrowings")]
        public BorrowingsDto? Borrowings { get; set; }

        /// <summary>
        /// 8. Vốn chủ sở hữu - OWNERS_EQUITY
        /// </summary>
        [JsonPropertyName("equity")]
        public EquityDto? Equity { get; set; }
    }

    public class IncomeStatementDto
    {
        /// <summary>
        /// 9. Thu nhập hoạt động (Ngân hàng) - OPERATING_INCOME
        /// </summary>
        [JsonPropertyName("bankOperatingIncome")]
        public BankOperatingIncomeDto? BankOperatingIncome { get; set; }

        /// <summary>
        /// 10. Lợi nhuận HĐKD bảo hiểm - INSURANCE_BUSINESS_PROFIT
        /// </summary>
        [JsonPropertyName("insuranceBusiness")]
        public InsuranceBusinessDto? InsuranceBusiness { get; set; }

        /// <summary>
        /// 11. Doanh thu HĐKD chính (Chứng khoán) - SECURITIES_REVENUE
        /// </summary>
        [JsonPropertyName("securitiesRevenue")]
        public SecuritiesRevenueDto? SecuritiesRevenue { get; set; }

        /// <summary>
        /// 12. Lợi nhuận gộp - GROSS_PROFIT
        /// </summary>
        [JsonPropertyName("grossProfit")]
        public GrossProfitDto? GrossProfit { get; set; }

        /// <summary>
        /// 13. Chi phí kinh doanh - BUSINESS_EXPENSES
        /// </summary>
        [JsonPropertyName("expenses")]
        public ExpensesDto? Expenses { get; set; }

        /// <summary>
        /// 14. Lợi nhuận trước thuế - PROFIT_BEFORE_TAX
        /// </summary>
        [JsonPropertyName("profitBeforeTax")]
        public ProfitBeforeTaxDto? ProfitBeforeTax { get; set; }

        /// <summary>
        /// 15. Lợi nhuận sau thuế và AFS - PROFIT_AFTER_TAX_AND_AFS
        /// </summary>
        [JsonPropertyName("profitAfterTaxAndAFS")]
        public ProfitAfterTaxAndAfsDto? ProfitAfterTaxAndAfs { get; set; }

        /// <summary>
        /// 16. LNST công ty mẹ - PROFIT_AFTER_TAX_PARENT_COMPANY
        /// </summary>
        [JsonPropertyName("parentCompanyNetProfit")]
        public ParentCompanyNetProfitDto? ParentCompanyNetProfit { get; set; }
    }

    #region Balance Sheet DTOs - 8 DTOs

    /// <summary>
    /// 1. Tài sản ngắn hạn
    /// </summary>
    public class ShortTermAssetsDto
    {
        [JsonPropertyName("cash")]
        public decimal? Cash { get; set; }

        [JsonPropertyName("financialInvestments")]
        public decimal? FinancialInvestments { get; set; }

        [JsonPropertyName("receivables")]
        public decimal? Receivables { get; set; }

        [JsonPropertyName("inventories")]
        public decimal? Inventories { get; set; }

        [JsonPropertyName("otherAssets")]
        public decimal? OtherAssets { get; set; }
    }

    /// <summary>
    /// 2. Tài sản dài hạn
    /// </summary>
    public class LongTermAssetsDto
    {
        [JsonPropertyName("receivables")]
        public decimal? Receivables { get; set; }

        [JsonPropertyName("fixedAssets")]
        public decimal? FixedAssets { get; set; }

        [JsonPropertyName("investmentProperty")]
        public decimal? InvestmentProperty { get; set; }

        [JsonPropertyName("longTermAssetsInProgress")]
        public decimal? LongTermAssetsInProgress { get; set; }

        [JsonPropertyName("financialInvestments")]
        public decimal? FinancialInvestments { get; set; }

        [JsonPropertyName("otherAssets")]
        public decimal? OtherAssets { get; set; }
    }

    /// <summary>
    /// 3. Tổng tài sản (Ngân hàng)
    /// </summary>
    public class BankAssetsDto
    {
        [JsonPropertyName("depositsAtCentralBank")]
        public decimal? DepositsAtCentralBank { get; set; }

        [JsonPropertyName("depositsAtOtherCreditInstitutions")]
        public decimal? DepositsAtOtherCreditInstitutions { get; set; }

        [JsonPropertyName("tradingSecurities")]
        public decimal? TradingSecurities { get; set; }

        [JsonPropertyName("loansToCustomers")]
        public decimal? LoansToCustomers { get; set; }

        [JsonPropertyName("investmentSecurities")]
        public decimal? InvestmentSecurities { get; set; }

        [JsonPropertyName("otherAssets")]
        public decimal? OtherAssets { get; set; }
    }

    /// <summary>
    /// 4. Tài sản tài chính ngắn hạn (Chứng khoán)
    /// </summary>
    public class ShortTermFinancialAssetsDto
    {
        [JsonPropertyName("cash")]
        public decimal? Cash { get; set; }

        [JsonPropertyName("loans")]
        public decimal? Loans { get; set; }

        [JsonPropertyName("other")]
        public decimal? Other { get; set; }
    }

    /// <summary>
    /// 5. Tài sản tự doanh và nguồn vốn
    /// </summary>
    public class TradingAndCapitalAssetsDto
    {
        [JsonPropertyName("heldToMaturity")]
        public decimal? HeldToMaturity { get; set; }

        [JsonPropertyName("availableForSale")]
        public decimal? AvailableForSale { get; set; }

        [JsonPropertyName("fvtpl")]
        public decimal? FVTPL { get; set; }
    }

    /// <summary>
    /// 6. Nợ phải trả
    /// </summary>
    public class LiabilitiesDto
    {
        [JsonPropertyName("shortTerm")]
        public decimal? ShortTerm { get; set; }

        [JsonPropertyName("longTerm")]
        public decimal? LongTerm { get; set; }

        [JsonPropertyName("govAndCentralBankDebt")]
        public decimal? GovAndCentralBankDebt { get; set; }

        [JsonPropertyName("borrowingsFromOtherCreditInstitutions")]
        public decimal? BorrowingsFromOtherCreditInstitutions { get; set; }

        [JsonPropertyName("customerDeposits")]
        public decimal? CustomerDeposits { get; set; }

        [JsonPropertyName("issuedValuePapers")]
        public decimal? IssuedValuePapers { get; set; }

        [JsonPropertyName("otherLiabilities")]
        public decimal? OtherLiabilities { get; set; }
    }

    /// <summary>
    /// 7. Nợ vay
    /// </summary>
    public class BorrowingsDto
    {
        [JsonPropertyName("shortTermBorrowings")]
        public decimal? ShortTermBorrowings { get; set; }

        [JsonPropertyName("longTermBorrowings")]
        public decimal? LongTermBorrowings { get; set; }
    }

    /// <summary>
    /// 8. Vốn chủ sở hữu
    /// </summary>
    public class EquityDto
    {
        [JsonPropertyName("contributedCapital")]
        public decimal? ContributedCapital { get; set; }

        [JsonPropertyName("retainedEarnings")]
        public decimal? RetainedEarnings { get; set; }

        [JsonPropertyName("treasuryShares")]
        public decimal? TreasuryShares { get; set; }

        [JsonPropertyName("otherCapital")]
        public decimal? OtherCapital { get; set; }

        [JsonPropertyName("creditInstitutionFunds")]
        public decimal? CreditInstitutionFunds { get; set; }
    }

    #endregion

    #region Income Statement DTOs - 8 DTOs

    /// <summary>
    /// 9. Thu nhập hoạt động (Ngân hàng)
    /// </summary>
    public class BankOperatingIncomeDto
    {
        [JsonPropertyName("netInterestIncome")]
        public decimal? NetInterestIncome { get; set; }

        [JsonPropertyName("serviceFeeIncome")]
        public decimal? ServiceFeeIncome { get; set; }

        [JsonPropertyName("tradingIncome")]
        public decimal? TradingIncome { get; set; }

        [JsonPropertyName("otherIncome")]
        public decimal? OtherIncome { get; set; }
    }

    /// <summary>
    /// 10. Lợi nhuận HĐKD bảo hiểm
    /// </summary>
    public class InsuranceBusinessDto
    {
        [JsonPropertyName("operatingProfit")]
        public decimal? OperatingProfit { get; set; }

        [JsonPropertyName("netOperatingRevenue")]
        public decimal? NetOperatingRevenue { get; set; }

        [JsonPropertyName("operatingExpenses")]
        public decimal? OperatingExpenses { get; set; }
    }

    /// <summary>
    /// 11. Doanh thu HĐKD chính (Chứng khoán)
    /// </summary>
    public class SecuritiesRevenueDto
    {
        [JsonPropertyName("brokerageAndCustodyRevenue")]
        public decimal? BrokerageAndCustodyRevenue { get; set; }

        [JsonPropertyName("lendingRevenue")]
        public decimal? LendingRevenue { get; set; }

        [JsonPropertyName("tradingAndCapitalRevenue")]
        public decimal? TradingAndCapitalRevenue { get; set; }

        [JsonPropertyName("investmentBankingRevenue")]
        public decimal? InvestmentBankingRevenue { get; set; }
    }

    /// <summary>
    /// 12. Lợi nhuận gộp
    /// </summary>
    public class GrossProfitDto
    {
        [JsonPropertyName("grossProfit")]
        public decimal? GrossProfit { get; set; }

        [JsonPropertyName("netRevenue")]
        public decimal? NetRevenue { get; set; }

        [JsonPropertyName("costOfGoodsSold")]
        public decimal? CostOfGoodsSold { get; set; }
    }

    /// <summary>
    /// 13. Chi phí kinh doanh
    /// </summary>
    public class ExpensesDto
    {
        [JsonPropertyName("costOfGoodsSold")]
        public decimal? CostOfGoodsSold { get; set; }

        [JsonPropertyName("interestExpenses")]
        public decimal? InterestExpenses { get; set; }

        [JsonPropertyName("sellingExpenses")]
        public decimal? SellingExpenses { get; set; }

        [JsonPropertyName("financialExpenses")]
        public decimal? FinancialExpenses { get; set; }

        [JsonPropertyName("managementExpenses")]
        public decimal? ManagementExpenses { get; set; }
    }

    /// <summary>
    /// 14. Lợi nhuận trước thuế
    /// </summary>
    public class ProfitBeforeTaxDto
    {
        [JsonPropertyName("profitBeforeTax")]
        public decimal? ProfitBeforeTax { get; set; }

        [JsonPropertyName("operatingProfit")]
        public decimal? OperatingProfit { get; set; }

        [JsonPropertyName("financialProfit")]
        public decimal? FinancialProfit { get; set; }

        [JsonPropertyName("shareProfitOfAssociatesAndJoint")]
        public decimal? ShareProfitOfAssociatesAndJoint { get; set; }

        [JsonPropertyName("otherProfit")]
        public decimal? OtherProfit { get; set; }

        [JsonPropertyName("managementExpenses")]
        public decimal? ManagementExpenses { get; set; }

        /// <summary>
        /// Chi phí dự phòng
        /// </summary>
        [JsonPropertyName("provisionExpenses")]
        public decimal? ProvisionExpenses { get; set; }

        /// <summary>
        /// Chi phí hoạt động
        /// </summary>
        [JsonPropertyName("operatingExpenses")]
        public decimal? OperatingExpenses { get; set; }
    }

    /// <summary>
    /// 15. Lợi nhuận sau thuế và AFS
    /// </summary>
    public class ProfitAfterTaxAndAfsDto
    {
        [JsonPropertyName("profitAfterTaxAndAfs")]
        public decimal? ProfitAfterTaxAndAfs { get; set; }

        [JsonPropertyName("parentCompanyNetProfit")]
        public decimal? ParentCompanyNetProfit { get; set; }

        [JsonPropertyName("afsGains")]
        public decimal? AfsGains { get; set; }
    }

    /// <summary>
    /// 16. LNST công ty mẹ
    /// </summary>
    public class ParentCompanyNetProfitDto
    {
        [JsonPropertyName("parentCompanyNetProfit")]
        public decimal? ParentCompanyNetProfit { get; set; }

        [JsonPropertyName("profitBeforeTax")]
        public decimal? ProfitBeforeTax { get; set; }

        [JsonPropertyName("corporateIncomeTax")]
        public decimal? CorporateIncomeTax { get; set; }

        [JsonPropertyName("minorityInterests")]
        public decimal? MinorityInterests { get; set; }
    }

    #endregion

    #region Cash Flow DTOs - 1 DTO

    /// <summary>
    /// 17. Lưu chuyển tiền tệ
    /// </summary>
    public class CashFlowDto
    {
        [JsonPropertyName("netCashFlow")]
        public decimal? NetCashFlow { get; set; }

        [JsonPropertyName("operatingActivities")]
        public decimal? OperatingActivities { get; set; }

        [JsonPropertyName("investingActivities")]
        public decimal? InvestingActivities { get; set; }

        [JsonPropertyName("financingActivities")]
        public decimal? FinancingActivities { get; set; }
    }

    #endregion
}
