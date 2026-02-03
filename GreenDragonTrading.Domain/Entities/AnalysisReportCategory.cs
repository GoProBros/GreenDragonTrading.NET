using GreenDragonTrading.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities;

/// <summary>
/// Analysis Report Category entity - categorizes analysis reports (e.g., Macro, Industry, Company)
/// </summary>
[Table("analysis_report_categories")]
public class AnalysisReportCategory
{
    [Key]
    [Column("id", TypeName = "varchar(50)")]
    [MaxLength(50)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Code { get; set; } = null!;

    /// <summary>
    /// Category name
    /// </summary>
    [Required]
    [Column("name", TypeName = "varchar(200)")]
    [MaxLength(200)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Category description
    /// </summary>
    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    /// <summary>
    /// Category level (1-4)
    /// </summary>
    [Column("level")]
    public int Level { get; set; }

    /// <summary>
    /// Parent category ID for hierarchical structure
    /// </summary>
    [Column("parent_id", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string? ParentId { get; set; }

    /// <summary>
    /// Category status
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
    [ForeignKey(nameof(ParentId))]
    public virtual AnalysisReportCategory? ParentCategory { get; set; }

    public virtual ICollection<AnalysisReportCategory> ChildCategories { get; set; } = new List<AnalysisReportCategory>();

    public virtual ICollection<AnalysisReport> AnalysisReports { get; set; } = new List<AnalysisReport>();
}
