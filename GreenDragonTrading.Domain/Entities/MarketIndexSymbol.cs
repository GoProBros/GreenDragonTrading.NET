using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities
{
    /// <summary>
    /// Junction table that maps constituent symbols to a market index (e.g. VN30 ↔ VCB, HPG, …).
    /// </summary>
    [Table("market_index_symbols")]
    public class MarketIndexSymbol
    {
        /// <summary>
        /// References <see cref="MarketIndex.Code"/> (e.g. "VN30", "VNINDEX").
        /// Part of the composite primary key.
        /// </summary>
        [Required]
        [Column("index_code", TypeName = "varchar(20)")]
        [MaxLength(20)]
        public string IndexCode { get; set; } = null!;

        /// <summary>
        /// References <see cref="Symbol.Ticker"/> (e.g. "VCB", "HPG").
        /// Part of the composite primary key.
        /// </summary>
        [Required]
        [Column("ticker", TypeName = "varchar(20)")]
        [MaxLength(20)]
        public string Ticker { get; set; } = null!;

        /// <summary>
        /// Weight of this symbol in the index (0–1 or percentage, depending on index methodology).
        /// Null means unweighted / weight not tracked.
        /// </summary>
        [Column("weight", TypeName = "decimal(10,6)")]
        public decimal? Weight { get; set; }

        /// <summary>
        /// The date this symbol was added to the index.
        /// </summary>
        [Column("added_date", TypeName = "date")]
        public DateOnly? AddedDate { get; set; }

        /// <summary>
        /// The date this symbol was removed from the index. Null means still a constituent.
        /// </summary>
        [Column("removed_date", TypeName = "date")]
        public DateOnly? RemovedDate { get; set; }

        /// <summary>
        /// Whether this symbol is currently an active constituent of the index.
        /// </summary>
        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Display order within the index constituent list.
        /// </summary>
        [Column("display_order")]
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// When this record was created.
        /// </summary>
        [Column("created_at", TypeName = "timestamptz")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When this record was last updated.
        /// </summary>
        [Column("updated_at", TypeName = "timestamptz")]
        public DateTime? UpdatedAt { get; set; }

        // ── Navigation properties ──────────────────────────────────────────────
        [ForeignKey("IndexCode")]
        public virtual MarketIndex? MarketIndex { get; set; }

        [ForeignKey("Ticker")]
        public virtual Symbol? Symbol { get; set; }
    }
}
