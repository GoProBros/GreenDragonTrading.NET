namespace GreenDragonTrading.Domain.Constants.DNSE;

/// <summary>
/// Constants for DNSE API - Financial Report Codes and Labels Mapping
/// </summary>
public static class DnseConstants
{
    public const string API_BASE_URL = "https://api-bo.dnse.com.vn";
    public const string FINANCIAL_REPORT_ENDPOINT = "/senses-api/financial-report/details";
    public const string CORPORATE_ACTIONS_HISTORY_ENDPOINT = "/senses-api/corporate-actions/history";
    public const string CORPORATE_ACTIONS_UPCOMING_ENDPOINT = "/senses-api/corporate-actions";
    
    /// <summary>
    /// Cycle Types for financial reports
    /// </summary>
    public static class CycleType
    {
        public const string QUARTERLY = "quy";
        public const string YEARLY = "nam";
    }
    
    /// <summary>
    /// Financial Report Code Constants - 17 Codes from DNSE API
    /// Map DNSE codes to Vietnamese labels
    /// </summary>
    public static class ReportCodes
    {
        // 1. Tài sản ngắn hạn (dành cho DN thường)
        public const string SHORT_TERM_ASSETS = "SHORT_TERM_ASSETS";
        
        // 2. Tổng tài sản (chủ yếu dành cho ngân hàng)
        public const string TOTAL_ASSETS = "TOTAL_ASSETS";
        
        // 3. Tài sản dài hạn
        public const string LONG_TERM_ASSETS = "LONG_TERM_ASSETS";
        
        // 4. Nợ phải trả
        public const string ACCOUNTS_PAYABLE = "ACCOUNTS_PAYABLE";
        
        // 5. Vốn chủ sở hữu
        public const string OWNERS_EQUITY = "OWNERS_EQUITY";
        
        // 6. Thu nhập hoạt động (dành cho ngân hàng)
        public const string OPERATING_INCOME = "OPERATING_INCOME";
        
        // 7. Lợi nhuận HĐKD bảo hiểm (dành cho bảo hiểm)
        public const string INSURANCE_BUSINESS_PROFIT = "INSURANCE_BUSINESS_PROFIT";
        
        // 8. Lợi nhuận trước thuế
        public const string PROFIT_BEFORE_TAX = "PROFIT_BEFORE_TAX";
        
        // 9. Lợi nhuận sau thuế công ty mẹ
        public const string PROFIT_AFTER_TAX_PARENT_COMPANY = "PROFIT_AFTER_TAX_PARENT_COMPANY";
        
        // 10. Lưu chuyển tiền tệ
        public const string CASH_FLOW = "CASH_FLOW";
        
        // 11. Tài sản tài chính ngắn hạn (dành cho chứng khoán)
        public const string SHORT_TERM_FINANCIAL_ASSETS = "SHORT_TERM_FINANCIAL_ASSETS";
        
        // 12. Tài sản tự doanh và nguồn vốn (dành cho chứng khoán)
        public const string INVESTMENT_ASSETS = "INVESTMENT_ASSETS";
        
        // 13. Nợ vay
        public const string BORROWINGS = "BORROWINGS";
        
        // 14. Doanh thu HĐKD chính (dành cho chứng khoán)
        public const string MAIN_BUSINESS_OPERATING_PROFIT = "MAIN_BUSINESS_OPERATING_PROFIT";
        
        // 15. Chi phí kinh doanh (dành cho chứng khoán)
        public const string OPERATING_EXPENSES = "OPERATING_EXPENSES";
        
        // 16. Lợi nhuận sau thuế và AFS (dành cho chứng khoán)
        public const string PROFIT_AFTER_TAX_AND_AFS = "PROFIT_AFTER_TAX_AND_AFS";
        
        // 17. Lợi nhuận gộp
        public const string GROSS_PROFIT = "GROSS_PROFIT";
    }
}
