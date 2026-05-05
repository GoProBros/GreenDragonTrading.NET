using GreenDragonTrading.Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities
{
    [Table("proactive_alert_layer_b_settings")]
    public class ProactiveAlertLayerBSetting
    {
        [Key]
        [Column("id", TypeName = "integer")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("timeframe", TypeName = "varchar(10)")]
        [MaxLength(10)]
        public string Timeframe { get; set; } = ProactiveAlertLayerBDefaults.Timeframe;

        [Required]
        [Column("min_absolute_move_percent", TypeName = "numeric(18, 4)")]
        public decimal MinAbsoluteMovePercent { get; set; } = ProactiveAlertLayerBDefaults.MinAbsoluteMovePercent;

        [Required]
        [Column("atr_move_multiplier", TypeName = "numeric(18, 4)")]
        public decimal AtrMoveMultiplier { get; set; } = ProactiveAlertLayerBDefaults.AtrMoveMultiplier;

        [Required]
        [Column("min_volume_ratio", TypeName = "numeric(18, 4)")]
        public decimal MinVolumeRatio { get; set; } = ProactiveAlertLayerBDefaults.MinVolumeRatio;

        [Required]
        [Column("min_adx", TypeName = "numeric(18, 4)")]
        public decimal MinAdx { get; set; } = ProactiveAlertLayerBDefaults.MinAdx;

        [Required]
        [Column("max_indicator_snapshot_age_minutes", TypeName = "integer")]
        public int MaxIndicatorSnapshotAgeMinutes { get; set; } = ProactiveAlertLayerBDefaults.MaxIndicatorSnapshotAgeMinutes;

        [Required]
        [Column("created_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        [Column("updated_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        public static ProactiveAlertLayerBSetting CreateDefault(DateTimeOffset now)
        {
            return new ProactiveAlertLayerBSetting
            {
                Timeframe = ProactiveAlertLayerBDefaults.Timeframe,
                MinAbsoluteMovePercent = ProactiveAlertLayerBDefaults.MinAbsoluteMovePercent,
                AtrMoveMultiplier = ProactiveAlertLayerBDefaults.AtrMoveMultiplier,
                MinVolumeRatio = ProactiveAlertLayerBDefaults.MinVolumeRatio,
                MinAdx = ProactiveAlertLayerBDefaults.MinAdx,
                MaxIndicatorSnapshotAgeMinutes = ProactiveAlertLayerBDefaults.MaxIndicatorSnapshotAgeMinutes,
                CreatedAt = now,
                UpdatedAt = now
            };
        }
    }
}
