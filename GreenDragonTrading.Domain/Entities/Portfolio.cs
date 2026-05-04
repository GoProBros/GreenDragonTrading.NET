using GreenDragonTrading.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities
{
    [Table("portfolios")]
    public class Portfolio
    {
        [Key]
        [Column("id", TypeName = "integer")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("user_id", TypeName = "uuid")]
        public Guid UserId { get; set; }

        [MaxLength(100)]
        [Column("name", TypeName = "varchar(100)")]
        public string? Name { get; set; } = string.Empty;

        [Column("description", TypeName = "text")]
        public string? Description { get; set; }

        [Required]
        [Column("status", TypeName = "smallint")]
        public CommonStatus Status { get; set; } = CommonStatus.Active;

        [Required]
        [Column("created_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        [MaxLength(20)]
        [Column("ticker", TypeName = "varchar(20)")]
        public string Ticker { get; set; } = string.Empty;

        // Navigation Property
        [ForeignKey("Ticker")]
        public virtual Symbol Symbol { get; set; } = default!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = default!;

        public virtual ICollection<TradingTransaction> Transactions { get; set; } = new List<TradingTransaction>();
    }
}
