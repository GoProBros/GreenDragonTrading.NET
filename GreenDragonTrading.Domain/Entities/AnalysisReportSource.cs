using GreenDragonTrading.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities;

/// <summary>
/// Analysis Report Source entity - represents the source of analysis reports (e.g., SSI, VCSC, VNDirect)
/// </summary>
[Table("analysis_report_sources")]
public class AnalysisReportSource
{
    [Key]
    [Column("id", TypeName = "varchar(50)")]
    [MaxLength(50)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Code { get; set; } = null!;

    /// <summary>
    /// Source name
    /// </summary>
    [Required]
    [Column("name", TypeName = "varchar(200)")]
    [MaxLength(200)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Source description
    /// </summary>
    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    /// <summary>
    /// Source website URL
    /// </summary>
    [Column("website", TypeName = "varchar(500)")]
    [MaxLength(500)]
    public string? Website { get; set; }

    /// <summary>
    /// Source logo URL
    /// </summary>
    [Column("logo_url", TypeName = "varchar(500)")]
    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Source status
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
    public virtual ICollection<AnalysisReport> AnalysisReports { get; set; } = new List<AnalysisReport>();
}
