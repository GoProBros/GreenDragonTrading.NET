using GreenDragonTrading.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities
{
    [Table("chat_participants")]
    public class ChatParticipant
    {
        [Key]
        [Column("id", TypeName = "integer")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("session_id", TypeName = "integer")]
        public int SessionId { get; set; }

        [Required]
        [Column("user_id", TypeName = "uuid")]
        public Guid UserId { get; set; }

        [Required]
        [Column("role", TypeName = "smallint")]
        public ChatRole Role { get; set; } = ChatRole.Member;

        [Required]
        [Column("last_read_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset LastReadAt { get; set; } = DateTimeOffset.UtcNow;

        [Column("last_read_message_id", TypeName = "integer")]
        public int? LastReadMessageId { get; set; }

        [Required]
        [Column("joined_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;

        [ForeignKey("SessionId")]
        public virtual ChatSession Session { get; set; } = default!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = default!;
    }
}
