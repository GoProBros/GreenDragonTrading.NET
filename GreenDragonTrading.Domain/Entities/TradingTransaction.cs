using GreenDragonTrading.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities
{
    [Table("trading_transactions")]
    public class TradingTransaction
    {
        [Key]
        [Column("id", TypeName = "integer")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("portfolio_id", TypeName = "integer")]
        public int PortfolioId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("ticker", TypeName = "varchar(20)")]
        public string Ticker { get; set; } = string.Empty;

        [Required]
        [Column("transaction_type", TypeName = "smallint")]
        public TransactionSide Side { get; set; } 

        [Column("quantity", TypeName = "numeric(18, 4)")]
        public decimal? Quantity { get; set; } = null;

        [Column("price", TypeName = "numeric(18, 4)")]
        public decimal? Price { get; set; } = null;

        [Column("fee", TypeName = "numeric(18, 4)")]
        public decimal? Fee { get; set; } = null;

        [Column("tax", TypeName = "numeric(18, 4)")]
        public decimal? Tax { get; set; } = null;

        [Required]
        [Column("transaction_date", TypeName = "timestamp with time zone")]
        public DateTimeOffset TransactionDate { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        [Column("recorded_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset RecordedAt { get; set; } = DateTimeOffset.UtcNow;

        [Column("note", TypeName = "text")]
        public string? Note { get; set; }

        [Column("original_message", TypeName = "jsonb")]
        public string? OriginalMessage { get; set; }

        [ForeignKey("PortfolioId")]
        public virtual Portfolio Portfolio { get; set; } = default!;

        [ForeignKey("Ticker")]
        public virtual Symbol Symbol { get; set; } = default!;
    }
}
