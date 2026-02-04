using GreenDragonTrading.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities;

/// <summary>
/// Analysis Report entity - aggregates analysis reports from multiple sources
/// </summary>
[Table("analysis_reports")]
public class AnalysisReport
{
    [Key]
    [Column("id", TypeName = "uuid")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Report source ID
    /// </summary>
    [Required]
    [Column("source_id", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string SourceId { get; set; } = null!;

    /// <summary>
    /// Report category ID
    /// </summary>
    [Required]
    [Column("category_id", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string CategoryId { get; set; } = null!;

    /// <summary>
    /// Report title
    /// </summary>
    [Required]
    [Column("title", TypeName = "varchar(500)")]
    [MaxLength(500)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// Report description/summary
    /// </summary>
    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    /// <summary>
    /// Related symbol tickers (optional - for tagging multiple symbols)
    /// </summary>
    [Column("tickers", TypeName = "varchar(20)[]")]
    public string[]? Tickers { get; set; }

    /// <summary>
    /// Related sector ID (optional - for tagging)
    /// </summary>
    [Column("sector_id", TypeName = "varchar(10)")]
    [MaxLength(10)]
    public string? SectorId { get; set; }

    /// <summary>
    /// Report publish date (from source)
    /// </summary>
    [Column("publish_date", TypeName = "timestamp with time zone")]
    public DateTimeOffset? PublishDate { get; set; }

    /// <summary>
    /// File path
    /// </summary>
    [Column("file_path", TypeName = "varchar(500)")]
    [MaxLength(500)]
    public string? FilePath { get; set; } = null!;

    /// <summary>
    /// Original file name
    /// </summary>
    [Column("original_file_name", TypeName = "varchar(500)")]
    [MaxLength(500)]
    public string? OriginalFileName { get; set; } = null!;

    /// <summary>
    /// File extension
    /// </summary>
    [Column("file_extension", TypeName = "varchar(10)")]
    [MaxLength(10)]
    public string? FileExtension { get; set; } = null!;

    /// <summary>
    /// MIME type
    /// </summary>
    [Column("mime_type", TypeName = "varchar(100)")]
    [MaxLength(100)]
    public string? MimeType { get; set; } = null!;

    /// <summary>
    /// File size in bytes
    /// </summary>
    [Column("file_size", TypeName = "bigint")]
    public long? FileSize { get; set; }

    /// <summary>
    /// Uploaded by user ID
    /// </summary>
    [Column("uploaded_by", TypeName = "uuid")]
    public Guid? UploadedBy { get; set; }

    /// <summary>
    /// Report status
    /// </summary>
    [Required]
    [Column("status", TypeName = "smallint")]
    public CommonStatus Status { get; set; } = CommonStatus.Active;

    /// <summary>
    /// Created date
    /// </summary>
    [Required]
    [Column("created_at", TypeName = "timestamp with time zone")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Updated date
    /// </summary>
    [Column("updated_at", TypeName = "timestamp with time zone")]
    public DateTimeOffset? UpdatedAt { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(SourceId))]
    public virtual AnalysisReportSource Source { get; set; } = null!;

    [ForeignKey(nameof(CategoryId))]
    public virtual AnalysisReportCategory Category { get; set; } = null!;

    [ForeignKey(nameof(SectorId))]
    public virtual Sector? Sector { get; set; }

    [ForeignKey(nameof(UploadedBy))]
    public virtual User? Uploader { get; set; }
}
