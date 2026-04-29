using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities
{
    [Table("corporate_actions")]
    public class CorporateAction
    {
        [Key]
        [Column("event_id", TypeName = "integer")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int EventId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("ticker", TypeName = "varchar(20)")]
        public string Ticker { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("name", TypeName = "varchar(255)")]
        public string Name { get; set; } = string.Empty;

        [Column("title", TypeName = "text")]
        public string? Title { get; set; }

        [Column("title_event", TypeName = "text")]
        public string? TitleEvent { get; set; }

        [Column("content", TypeName = "text")]
        public string? Content { get; set; }

        [Column("note", TypeName = "text")]
        public string? Note { get; set; }

        [Column("url", TypeName = "varchar(500)")]
        public string? Url { get; set; }

        [Column("ex_rights_date", TypeName = "timestamp with time zone")]
        public DateTimeOffset? ExRightsDate { get; set; }

        [Column("record_date", TypeName = "timestamp with time zone")]
        public DateTimeOffset? RecordDate { get; set; }

        [Column("action_date", TypeName = "timestamp with time zone")]
        public DateTimeOffset? ActionDate { get; set; }

        [Required]
        [Column("event_type", TypeName = "smallint")]
        public int EventType { get; set; } 

        [Required]
        [Column("created_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        [Column("updated_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        [ForeignKey("Ticker")]
        public virtual Symbol Symbol { get; set; } = default!;
    }
}
