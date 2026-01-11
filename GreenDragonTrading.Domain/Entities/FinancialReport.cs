using GreenDragonTrading.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities
{
    /// <summary>
    /// Báo cáo tài chính
    /// </summary>
    [Table("financial_reports")]
    public class FinancialReport
    {
        [Key]
        [Column("id", TypeName = "uuid")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [Column("ticker", TypeName = "varchar(20)")]
        [MaxLength(20)]
        public string Ticker { get; set; } = null!;

        [Required]
        [Column("year", TypeName = "integer")]
        public int Year { get; set; }

        [Required]
        [Column("period", TypeName = "smallint")]
        public ReportPeriod Period { get; set; }

        // Attached Files
        [Column("file_path", TypeName = "varchar(500)")]
        [MaxLength(500)]
        public string? FilePath { get; set; }

        [Column("file_size", TypeName = "bigint")]
        public long? FileSize { get; set; }

        #region Balance Sheet - Bảng cân đối kế toán
        /// <summary>
        /// Tài sản ngắn hạn
        /// </summary>
        [Column("short_term_assets", TypeName = "decimal(20,2)")]
        public decimal? ShortTermAssets { get; set; }

        /// <summary>
        /// Tiền và các khoản tương đương tiền
        /// </summary>
        [Column("cash_and_cash_equivalents", TypeName = "decimal(20,2)")]
        public decimal? CashAndCashExchangeable { get; set; }

        /// <summary>
        /// Đầu tư tài chính ngắn hạn
        /// </summary>
        [Column("short_term_financial_investments", TypeName = "decimal(20,2)")]
        public decimal? ShortTermFinancialInvestments { get; set; }

        /// <summary>
        /// Các khoản phải thu ngắn hạn
        /// </summary>
        [Column("short_term_receivables", TypeName = "decimal(20,2)")]
        public decimal? ShortTermReceivables { get; set; }

        /// <summary>
        /// Hàng tồn kho
        /// </summary>
        [Column("inventories", TypeName = "decimal(20,2)")]
        public decimal? Inventories { get; set; }

        /// <summary>
        /// Tài sản ngắn hạn khác
        /// </summary>
        [Column("provision_for_decline_in_inventory", TypeName = "decimal(20,2)")]
        public decimal? ProvisionForDeclineInInventory { get; set; }

        /// <summary>
        /// Tài sản dài hạn
        /// </summary>
        [Column("long_term_assets", TypeName = "decimal(20,2)")]
        public decimal? LongTermAssets { get; set; }

        /// <summary>
        /// Các khoản phải thu dài hạn
        /// </summary>
        [Column("long_term_receivables", TypeName = "decimal(20,2)")]
        public decimal? LongTermReceivables { get; set; }

        /// <summary>
        /// Tài sản cố định
        /// </summary>
        [Column("fixed_assets", TypeName = "decimal(20,2)")]
        public decimal? FixedAssets { get; set; }

        /// <summary>
        /// Tổng tài sản
        /// </summary>
        [Column("total_assets", TypeName = "decimal(20,2)")]
        public decimal? TotalAssets { get; set; }

        /// <summary>
        /// Nợ phải trả
        /// </summary>
        [Column("liabilities", TypeName = "decimal(20,2)")]
        public decimal? Liabilities { get; set; }

        /// <summary>
        /// Nợ ngắn hạn
        /// </summary>
        [Column("short_term_liabilities", TypeName = "decimal(20,2)")]
        public decimal? ShortTermLiabilities { get; set; }

        /// <summary>
        /// Nợ dài hạn
        /// </summary>
        [Column("long_term_liabilities", TypeName = "decimal(20,2)")]
        public decimal? LongTermLiabilities { get; set; }

        /// <summary>
        /// Vốn chủ sở hữu
        /// </summary>
        [Column("owner_equity", TypeName = "decimal(20,2)")]
        public decimal? OwnerEquity { get; set; }

        /// <summary>
        /// Nguồn vốn khác
        /// </summary>
        [Column("other_funds", TypeName = "decimal(20,2)")]
        public decimal? OtherFunds { get; set; }

        /// <summary>
        /// Tổng nguồn vốn
        /// </summary>
        [Column("total_resources", TypeName = "decimal(20,2)")]
        public decimal? TotalResources { get; set; }
        #endregion Balance Sheet - Bảng cân đối kế toán

        #region Income Statement - Báo cáo kết quả kinh doanh
        /// <summary>
        /// Doanh thu
        /// </summary>
        [Column("revenue", TypeName = "decimal(20,2)")]
        public decimal? Revenue { get; set; }

        /// <summary>
        /// Giảm trừ doanh thu
        /// </summary>
        [Column("revenue_deductions", TypeName = "decimal(20,2)")]
        public decimal? RevenueDeductions { get; set; }

        /// <summary>
        /// Doanh thu thuần
        /// </summary>
        [Column("net_revenue", TypeName = "decimal(20,2)")]
        public decimal? NetRevenue { get; set; }

        /// <summary>
        /// Giá vốn hàng bán
        /// </summary>
        [Column("cost_of_goods_sold", TypeName = "decimal(20,2)")]
        public decimal? CostOfGoodsSold { get; set; }

        /// <summary>
        /// Lợi nhuận gộp
        /// </summary>
        [Column("gross_profit", TypeName = "decimal(20,2)")]
        public decimal? GrossProfit { get; set; }

        /// <summary>
        /// Doanh thu từ hoạt động tài chính
        /// </summary>
        [Column("revenue_from_financial_activities", TypeName = "decimal(20,2)")]
        public decimal? RevenueFromFinancialActivities { get; set; }

        /// <summary>
        /// Chi phí tài chính
        /// </summary>
        [Column("financial_expenses", TypeName = "decimal(20,2)")]
        public decimal? FinancialExpenses { get; set; }

        /// <summary>
        /// Chi phí quản lý doanh nghiệp
        /// </summary>
        [Column("general_and_administration_expenses", TypeName = "decimal(20,2)")]
        public decimal? GeneralAndAdministrationExpenses { get; set; }

        /// <summary>
        /// Lợi nhuận thuần
        /// </summary>
        [Column("net_profit", TypeName = "decimal(20,2)")]
        public decimal? NetProfit { get; set; }

        /// <summary>
        /// Thu nhập khác
        /// </summary>
        [Column("other_income", TypeName = "decimal(20,2)")]
        public decimal? OtherIncome { get; set; }

        /// <summary>
        /// Chi phí khác
        /// </summary>
        [Column("other_expense", TypeName = "decimal(20,2)")]
        public decimal? OtherExpense { get; set; }

        /// <summary>
        /// Lợi nhuận khác
        /// </summary>
        [Column("other_profit", TypeName = "decimal(20,2)")]
        public decimal? OtherProfit { get; set; }

        /// <summary>
        /// Tổng lợi nhuận trước thuế
        /// </summary>
        [Column("profit_before_tax", TypeName = "decimal(20,2)")]
        public decimal? ProfitBeforeTax { get; set; }

        /// <summary>
        /// Thuế TNDN
        /// </summary>
        [Column("income_tax_expense", TypeName = "decimal(20,2)")]
        public decimal? IncomeTaxExpense { get; set; }

        /// <summary>
        /// Tổng lợi nhuận sau thuế
        /// </summary>
        [Column("profit_after_tax", TypeName = "decimal(20,2)")]
        public decimal? ProfitAfterTax { get; set; }
        #endregion Income Statement - Báo cáo kết quả kinh doanh

        #region Cash Flow Statement - Báo cáo lưu chuyển tiền tệ
        /// <summary>
        /// Tiền từ hoạt động kinh doanh
        /// </summary>
        [Column("cash_from_operating", TypeName = "decimal(20,2)")]
        public decimal? CashFromOperating { get; set; }

        /// <summary>
        /// Tiền từ hoạt động đầu tư
        /// </summary>
        [Column("cash_from_investing", TypeName = "decimal(20,2)")]
        public decimal? CashFromInvesting { get; set; }

        /// <summary>
        /// Tiền từ hoạt động tài chính
        /// </summary>
        [Column("cash_from_financing", TypeName = "decimal(20,2)")]
        public decimal? CashFromFinancing { get; set; }

        /// <summary>
        /// Lưu chuyển thuần trong kỳ
        /// </summary>
        [Column("net_cash_flow", TypeName = "decimal(20,2)")]
        public decimal? NetCashFlow { get; set; }

        /// <summary>
        /// Tiền và tương đương tiền đầu kỳ
        /// </summary>
        [Column("beginning_cash", TypeName = "decimal(20,2)")]
        public decimal? BeginningCash { get; set; }

        /// <summary>
        /// Tiền và tương đương tiền cuối kỳ
        /// </summary>
        [Column("ending_cash", TypeName = "decimal(20,2)")]
        public decimal? EndingCash { get; set; }
        #endregion Cash Flow Statement - Báo cáo lưu chuyển tiền tệ

        [Required]
        [Column("status", TypeName = "smallint")]
        public FinancialReportStatus Status { get; set; } = FinancialReportStatus.Completed;

        [Required]
        [Column("created_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        [Column("updated_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation Property
        [ForeignKey("Ticker")]
        public virtual Symbol Symbol { get; set; } = null!;
    }
}
