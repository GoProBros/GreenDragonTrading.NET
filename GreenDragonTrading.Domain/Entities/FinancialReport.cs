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

        [Column("file_path", TypeName = "varchar(500)")]
        [MaxLength(500)]
        public string? FilePath { get; set; }

        [Column("file_size", TypeName = "bigint")]
        public long? FileSize { get; set; }

        [Column("content_type", TypeName = "varchar(100)")]
        [MaxLength(100)]
        public string? ContentType { get; set; }

        /// <summary>
        /// Chỉ số tài chính chính dạng JSON
        /// </summary>
        [Column("key_metrics", TypeName = "jsonb")]
        public string? KeyMetrics { get; set; }

        [Required]
        [Column("status", TypeName = "smallint")]
        public FinancialReportStatus Status { get; set; } = FinancialReportStatus.Pending;

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
