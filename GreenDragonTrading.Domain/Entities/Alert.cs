using GreenDragonTrading.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities
{
    [Table("alerts")]
    public class Alert
    {
        [Key]
        [Column("id", TypeName = "integer")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("user_id", TypeName = "uuid")]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("ticker", TypeName = "varchar(20)")]
        public string Ticker { get; set; } = string.Empty;

        [Required]
        [Column("alert_type", TypeName = "smallint")]
        public AlertType Type { get; set; } 

        [Required]
        [Column("condition_type", TypeName = "smallint")]
        public ConditionType Condition { get; set; }

        [Column("change_percentage", TypeName = "numeric(18, 2)")]
        public decimal? ChangePercentage { get; set; }

        [Column("current_price", TypeName = "numeric(18, 4)")]
        public decimal? CurrentPrice { get; set; }

        [Column("threshold_value", TypeName = "numeric(18, 4)")]
        public decimal? ThresholdValue { get; set; }

        [Column("volume_time_frame", TypeName = "smallint")]
        public VolumeTimeFrame? VolumeTimeFrame { get; set; }

        [Column("volume_lookback_bars", TypeName = "smallint")]
        public int? VolumeLookbackBars { get; set; }

        [MaxLength(255)]
        [Column("name", TypeName = "varchar(255)")]
        public string? Name { get; set; }

        [Required]
        [Column("is_active", TypeName = "boolean")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("is_triggered", TypeName = "boolean")]
        public bool IsTriggered { get; set; } = false;

        [Column("last_triggered_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset? LastTriggeredAt { get; set; }

        [Column("chat_session_id", TypeName = "integer")]
        public int? ChatSessionId { get; set; }

        [Column("message_template", TypeName = "jsonb")]
        public string? MessageTemplate { get; set; }

        [Required]
        [Column("notify_via", TypeName = "smallint")]
        public NotificationChannel NotifyVia { get; set; }

        [Required]
        [Column("created_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        [Column("updated_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = default!;

        [ForeignKey("Ticker")]
        public virtual Symbol Symbol { get; set; } = default!;
    }
}