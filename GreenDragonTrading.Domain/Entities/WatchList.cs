using GreenDragonTrading.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities
{
    [Table("watch_lists")]
    public class WatchList
    {
        [Key]
        [Column("id", TypeName = "integer")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("user_id", TypeName = "uuid")]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("name", TypeName = "varchar(200)")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("tickers", TypeName = "jsonb")]
        public string Tickers { get; set; } = "[]";

        [Required]
        [Column("status", TypeName = "smallint")]
        public CommonStatus Status { get; set; } = CommonStatus.Active;

        [Column("created_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Column("updated_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset? UpdatedAt { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}
