using GreenDragonTrading.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace GreenDragonTrading.Domain.Entities
{
    /// <summary>
    /// Báo cáo tài chính - Financial Report
    /// Lưu dữ liệu dạng JSON để linh hoạt với các loại báo cáo khác nhau
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
        [Column("quarter", TypeName = "integer")]
        public int Quarter { get; set; }

        [Required]
        [Column("period", TypeName = "smallint")]
        public ReportPeriod Period { get; set; }

        /// <summary>
        /// Dữ liệu báo cáo tài chính dạng JSON
        /// Chứa toàn bộ thông tin bảng cân đối kế toán, báo cáo kết quả kinh doanh, lưu chuyển tiền tệ
        /// </summary>
        [Required]
        [Column("report_data", TypeName = "jsonb")]
        public FinancialReportData ReportData { get; set; } = new();

        /// <summary>
        /// Đường dẫn file PDF báo cáo tài chính
        /// </summary>
        [Column("file_path", TypeName = "varchar(500)")]
        [MaxLength(500)]
        public string? FilePath { get; set; }

        /// <summary>
        /// Kích thước file (bytes)
        /// </summary>
        [Column("file_size", TypeName = "bigint")]
        public long? FileSize { get; set; }

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
