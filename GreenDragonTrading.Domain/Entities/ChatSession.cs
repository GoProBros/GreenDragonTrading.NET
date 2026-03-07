using GreenDragonTrading.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities
{
    [Table("chat_sessions")]
    public class ChatSession
    {
        [Key]
        [Column("id", TypeName = "integer")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("title", TypeName = "varchar(255)")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column("session_type", TypeName = "smallint")]
        public ChatSessionType SessionType { get; set; }

        [Required]
        [Column("status", TypeName = "smallint")]
        public CommonStatus Status { get; set; } = CommonStatus.Active;

        [Column("created_by", TypeName = "uuid")]
        public Guid? CreatedBy { get; set; }

        [Required]
        [Column("created_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        [Column("updated_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation Properties
        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        public virtual ICollection<ChatParticipant> Participants { get; set; } = new List<ChatParticipant>();
    }
}
