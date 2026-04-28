using GreenDragonTrading.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities
{
    [Table("alert_templates")]
    public class AlertTemplate
    {
        [Key]
        [Column("id", TypeName = "integer")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("alert_type", TypeName = "smallint")]
        public AlertType? Type { get; set; }

        [Column("condition_type", TypeName = "smallint")]
        public ConditionType? Condition { get; set; }

        [Required]
        [Column("title_template", TypeName = "text")]
        public string TitleTemplate { get; set; } = string.Empty;

        [Required]
        [Column("body_template", TypeName = "text")]
        public string BodyTemplate { get; set; } = string.Empty;

        [Required]
        [Column("is_active", TypeName = "boolean")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("is_default", TypeName = "boolean")]
        public bool IsDefault { get; set; } = false;

        [Required]
        [Column("created_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        [Column("updated_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    }
}
