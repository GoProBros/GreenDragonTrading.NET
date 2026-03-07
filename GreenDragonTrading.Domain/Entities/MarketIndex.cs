using GreenDragonTrading.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities
{
    /// <summary>
    /// Represents a market index (e.g. VN30, VNINDEX, HNX30, UPCOM-INDEX).
    /// Stores index metadata; historical price data is kept in <see cref="MarketIndexOhlcv"/>.
    /// </summary>
    [Table("market_indices")]
    public class MarketIndex
    {
        /// <summary>
        /// Unique index code (e.g. "VNINDEX", "VN30", "HNX30").
        /// </summary>
        [Key]
        [Column("code", TypeName = "varchar(20)")]
        [MaxLength(20)]
        public string Code { get; set; } = null!;

        /// <summary>
        /// Full display name of the index.
        /// </summary>
        [Required]
        [Column("name", TypeName = "varchar(100)")]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Exchange this index belongs to (e.g. "hsx", "hnx", "upcom").
        /// </summary>
        [Required]
        [Column("exchange_code", TypeName = "varchar(20)")]
        [MaxLength(20)]
        public string ExchangeCode { get; set; } = null!;

        /// <summary>
        /// Short description or definition of the index.
        /// </summary>
        [Column("description", TypeName = "varchar(500)")]
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Whether this index is considered a primary benchmark (e.g. VNINDEX).
        /// </summary>
        [Column("is_benchmark")]
        public bool IsBenchmark { get; set; } = false;

        /// <summary>
        /// Active / Inactive status.
        /// </summary>
        [Required]
        [Column("status", TypeName = "smallint")]
        public CommonStatus Status { get; set; } = CommonStatus.Active;

        /// <summary>
        /// When this record was first created.
        /// </summary>
        [Column("created_at", TypeName = "timestamptz")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When this record was last updated.
        /// </summary>
        [Column("updated_at", TypeName = "timestamptz")]
        public DateTime? UpdatedAt { get; set; }

        // ── Navigation properties ──────────────────────────────────────────────
        [ForeignKey("ExchangeCode")]
        public virtual Exchange? Exchange { get; set; }
    }
}
