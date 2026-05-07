using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities
{
    /// <summary>
    /// Persistent trace of a proactive alert signal lifecycle.
    /// Steps are stored as JSONB for flexibility.
    /// </summary>
    [Table("proactive_alert_traces")]
    public class ProactiveAlertTrace
    {
        [Key]
        [Column("trace_id", TypeName = "varchar(32)")]
        [MaxLength(32)]
        public string TraceId { get; set; } = string.Empty;

        [Required]
        [Column("ticker", TypeName = "varchar(20)")]
        [MaxLength(20)]
        public string Ticker { get; set; } = string.Empty;

        [Column("final_result", TypeName = "varchar(50)")]
        [MaxLength(50)]
        public string? FinalResult { get; set; }

        [Column("candidate_user_count", TypeName = "integer")]
        public int CandidateUserCount { get; set; }

        [Column("eligible_user_count", TypeName = "integer")]
        public int EligibleUserCount { get; set; }

        [Column("notified_user_count", TypeName = "integer")]
        public int NotifiedUserCount { get; set; }

        /// <summary>
        /// Ordered list of pipeline steps stored as JSONB.
        /// </summary>
        [Column("steps", TypeName = "jsonb")]
        public List<ProactiveAlertTraceStep> Steps { get; set; } = new();

        [Required]
        [Column("created_at", TypeName = "timestamp with time zone")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("updated_at", TypeName = "timestamp with time zone")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// A single step within a proactive alert trace.
    /// </summary>
    public class ProactiveAlertTraceStep
    {
        [Column("step", TypeName = "varchar(50)")]
        [MaxLength(50)]
        public string Step { get; set; } = string.Empty;

        [Column("result", TypeName = "varchar(20)")]
        [MaxLength(20)]
        public string Result { get; set; } = string.Empty;

        [Column("fail_reason", TypeName = "text")]
        public string? FailReason { get; set; }

        /// <summary>
        /// Key-value detail payload for this step.
        /// </summary>
        [Column("detail", TypeName = "jsonb")]
        public Dictionary<string, object?>? Detail { get; set; }

        [Column("timestamp", TypeName = "timestamp with time zone")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
