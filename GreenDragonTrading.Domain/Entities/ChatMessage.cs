using GreenDragonTrading.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities
{
    [Table("chat_messages")]
    public class ChatMessage
    {
        [Key]
        [Column("id", TypeName = "integer")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("session_id", TypeName = "integer")]
        public int SessionId { get; set; }

        [Column("sender_id", TypeName = "uuid")]
        public Guid? SenderId { get; set; }

        [Required]
        [Column("content", TypeName = "text")]
        public string Content { get; set; } = string.Empty;

        [Required]
        [Column("message_type", TypeName = "smallint")]
        public ChatMessageType MessageType { get; set; } = ChatMessageType.Text;

        [Required]
        [Column("is_deleted", TypeName = "boolean")]
        public bool IsDeleted { get; set; } = false;

        [Column("file_url", TypeName = "varchar(500)")]
        public string? FileUrl { get; set; }

        [Column("file_name", TypeName = "varchar(255)")]
        public string? FileName { get; set; }

        [Column("file_size", TypeName = "bigint")]
        public long? FileSize { get; set; }

        [Required]
        [Column("created_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        [Column("updated_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation Properties
        [ForeignKey("SessionId")]
        public virtual ChatSession Session { get; set; } = default!;

        [ForeignKey("SenderId")]
        public virtual User? Sender { get; set; }
    }
}
