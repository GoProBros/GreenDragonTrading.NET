using System.Text.Json.Serialization;

namespace GreenDragonTrading.Domain.Constants.DNSE;

/// <summary>
/// Vietnamese label constants from DNSE API responses
/// Maps to series labels in financial report data
/// </summary>
public static class DnseLabels
{
    /// <summary>
    /// Balance Sheet - Short Term Assets Labels
    /// </summary>
    public static class ShortTermAssets
    {
        public const string Cash = "Tiền";
        public const string FinancialInvestments = "Đầu tư tài chính";
        public const string Receivables = "Phải thu";
        public const string OtherAssets = "Tài sản khác";
        public const string Inventories = "Hàng tồn kho";
    }

    /// <summary>
    /// Balance Sheet - Long Term Assets Labels
    /// </summary>
    public static class LongTermAssets
    {
        public const string FinancialInvestments = "Đầu tư tài chính";
        public const string FixedAssets = "Tài sản cố định";
        public const string InvestmentProperty = "Bất động sản đầu tư";
        public const string LongTermAssetsInProgress = "Tài sản dở dang";
        public const string OtherAssets = "Tài sản khác";
        public const string Receivables = "Phải thu";
    }

    /// <summary>
    /// Balance Sheet - Bank Assets Labels (for banking sector)
    /// </summary>
    public static class BankAssets
    {
        public const string DepositsAtCentralBank = "Tiền gửi tại NHNN";
        public const string DepositsAtOtherCreditInstitutions = "Tiền gửi các TCTD khác";
        public const string TradingSecurities = "Chứng khoán kinh doanh";
        public const string LoansToCustomers = "Cho vay khách hàng";
        public const string InvestmentSecurities = "Chứng khoán đầu tư";
        public const string OtherAssets = "Tài sản khác";
    }

    /// <summary>
    /// Balance Sheet - Short Term Financial Assets Labels (for securities sector)
    /// </summary>
    public static class ShortTermFinancialAssets
    {
        public const string Cash = "Tiền";
        public const string Loans = "Các khoản cho vay";
        public const string Other = "Khác";
    }

    /// <summary>
    /// Balance Sheet - Trading and Capital Assets Labels (for securities sector)
    /// </summary>
    public static class TradingAndCapitalAssets
    {
        public const string HeldToMaturity = "HTM";
        public const string AvailableForSale = "AFS";
        public const string FVTPL = "FVTPL";
    }

    /// <summary>
    /// Balance Sheet - Liabilities Labels
    /// </summary>
    public static class Liabilities
    {
        public const string ShortTerm = "Ngắn hạn";
        public const string LongTerm = "Dài hạn";
        public const string GovAndCentralBankDebt = "Nợ Chính phủ và NHNN";
        public const string BorrowingsFromOtherCreditInstitutions = "Vay các TCTD khác";
        public const string CustomerDeposits = "Tiền gửi của khách hàng";
        public const string IssuedValuePapers = "Phát hành giấy tờ có giá";
        public const string OtherLiabilities = "Nợ khác";
    }

    /// <summary>
    /// Balance Sheet - Borrowings Labels
    /// </summary>
    public static class Borrowings
    {
        public const string ShortTermBorrowings = "Vay ngắn hạn";
        public const string LongTermBorrowings = "Vay dài hạn";
    }

    /// <summary>
    /// Balance Sheet - Equity Labels
    /// </summary>
    public static class Equity
    {
        public const string ContributedCapital = "Vốn góp";
        public const string RetainedEarnings = "LNST chưa phân phối";
        public const string CreditInstitutionFunds = "Quỹ của TCTD";
        public const string TreasuryShares = "Cổ phiếu quỹ";
        public const string OtherCapital = "Vốn khác";
    }

    /// <summary>
    /// Income Statement - Bank Operating Income Labels (for banking sector)
    /// </summary>
    public static class BankOperatingIncome
    {
        public const string NetInterestIncome = "Thu nhập lãi thuần";
        public const string ServiceFeeIncome = "Lãi dịch vụ";
        public const string TradingIncome = "Hoạt động tự doanh";
        public const string OtherIncome = "Lãi khác";
    }

    /// <summary>
    /// Income Statement - Insurance Business Labels (for insurance sector)
    /// </summary>
    public static class InsuranceBusiness
    {
        public const string OperatingProfit = "Lợi nhuận HĐKD";
        public const string NetOperatingRevenue = "Doanh thu thuần HĐKD";
        public const string OperatingExpenses = "Các khoản chi HĐKD";
    }

    /// <summary>
    /// Income Statement - Securities Revenue Labels (for securities sector)
    /// </summary>
    public static class SecuritiesRevenue
    {
        public const string BrokerageAndCustodyRevenue = "DT môi giới và lưu ký";
        public const string LendingRevenue = "DT cho vay";
        public const string TradingAndCapitalRevenue = "DT tự doanh và nguồn vốn";
        public const string InvestmentBankingRevenue = "DT ngân hàng đầu tư";
    }

    /// <summary>
    /// Income Statement - Gross Profit Labels
    /// </summary>
    public static class GrossProfit
    {
        public const string Value = "Lợi nhuận gộp";
        public const string NetRevenue = "Doanh thu thuần";
        public const string CostOfGoodsSold = "Giá vốn hàng bán";
    }

    /// <summary>
    /// Income Statement - Expenses Labels
    /// </summary>
    public static class Expenses
    {
        public const string FinancialExpenses = "Chi phí tài chính";
        public const string ManagementExpenses = "Chi phí quản lý";
        public const string CostOfGoodsSold = "Chi phí giá vốn";
        public const string InterestExpenses = "Chi phí lãi vay";
        public const string SellingExpenses = "Chi phí bán hàng";
    }

    /// <summary>
    /// Income Statement - Profit Before Tax Labels
    /// </summary>
    public static class ProfitBeforeTax
    {
        public const string Value = "LN trước thuế";
        public const string ValueBank = "Lợi nhuận trước thuế";
        public const string OperatingProfit = "Lợi nhuận HĐKD";
        public const string OperatingProfitAlt = "LN kinh doanh";
        public const string OperatingProfitBank = "Thu nhập hoạt động";
        public const string FinancialProfit = "Lợi nhuận tài chính";
        public const string FinancialProfitAlt = "LN tài chính";
        public const string OtherProfit = "Lợi nhuận khác";
        public const string OtherProfitAlt = "LN khác";
        public const string ManagementExpenses = "Chi phí quản lý";
        public const string ShareProfitOfAssociatesAndJoint = "LN liên doanh, liên kết";
        public const string ProvisionExpenses = "Chi phí dự phòng";
        public const string OperatingExpenses = "Chi phí hoạt động";
    }

    /// <summary>
    /// Income Statement - Profit After Tax and AFS Labels (for securities sector)
    /// </summary>
    public static class ProfitAfterTaxAndAfs
    {
        public const string NetProfit = "LNST công ty mẹ";
        public const string AfsGains = "AFS";
        public const string Value = "LNST và AFS";
    }

    /// <summary>
    /// Income Statement - Parent Company Net Profit Labels
    /// </summary>
    public static class ParentCompanyNetProfit
    {
        public const string Value = "LNST công ty mẹ";
        public const string BeforeTax = "LN trước thuế";
        public const string CorporateIncomeTax = "Thuế TNDN";
        public const string MinorityInterests = "Lợi ích cổ đông thiểu số";
    }

    /// <summary>
    /// Cash Flow Statement Labels
    /// </summary>
    public static class CashFlow
    {
        public const string NetCashFlow = "Lưu chuyển tiền thuần";
        public const string OperatingActivities = "Hoạt động kinh doanh";
        public const string InvestingActivities = "Hoạt động đầu tư";
        public const string FinancingActivities = "Hoạt động tài chính";
    }

}
