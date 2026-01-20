using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities
{
    /// <summary>
    /// Dữ liệu OHLCV (Open, High, Low, Close, Volume)
    /// Lưu M1 và D1 raw data từ SSI vào TimescaleDB hypertable
    /// </summary>
    [Table("ohlcv")]
    public class Ohlcv
    {
        /// <summary>
        /// Thời gian của nến (timestamp)
        /// </summary>
        [Required]
        [Column("time", TypeName = "timestamptz")]
        public DateTime Time { get; set; }

        /// <summary>
        /// Mã chứng khoán
        /// </summary>
        [Required]
        [Column("ticker", TypeName = "varchar(20)")]
        [MaxLength(20)]
        public string Ticker { get; set; } = null!;

        /// <summary>
        /// Khung thời gian: M1 (1 phút), D1 (1 ngày)
        /// </summary>
        [Required]
        [Column("timeframe", TypeName = "varchar(10)")]
        [MaxLength(10)]
        public string Timeframe { get; set; } = null!;

        /// <summary>
        /// Giá mở cửa
        /// </summary>
        [Required]
        [Column("open", TypeName = "decimal(18,2)")]
        public decimal Open { get; set; }

        /// <summary>
        /// Giá cao nhất
        /// </summary>
        [Required]
        [Column("high", TypeName = "decimal(18,2)")]
        public decimal High { get; set; }

        /// <summary>
        /// Giá thấp nhất
        /// </summary>
        [Required]
        [Column("low", TypeName = "decimal(18,2)")]
        public decimal Low { get; set; }

        /// <summary>
        /// Giá đóng cửa
        /// </summary>
        [Required]
        [Column("close", TypeName = "decimal(18,2)")]
        public decimal Close { get; set; }

        /// <summary>
        /// Khối lượng giao dịch
        /// </summary>
        [Required]
        [Column("volume", TypeName = "bigint")]
        public long Volume { get; set; }

        /// <summary>
        /// Giá trị giao dịch
        /// </summary>
        [Column("value", TypeName = "decimal(18,2)")]
        public decimal? Value { get; set; }

        /// <summary>
        /// Số lượng giao dịch trong nến
        /// </summary>
        [Column("trades_count", TypeName = "integer")]
        public int? TradesCount { get; set; }

        /// <summary>
        /// Thời gian import vào DB
        /// </summary>
        [Required]
        [Column("created_at", TypeName = "timestamptz")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Nguồn dữ liệu: SSI_STREAMING (M1), SSI_API (D1), ...
        /// </summary>
        [Required]
        [Column("source", TypeName = "varchar(50)")]
        [MaxLength(50)]
        public string Source { get; set; } = "SSI";

        /// <summary>
        /// Flag indicating if this is preliminary data (for D1 real-time aggregation)
        /// Will be overwritten by official data from SSI API later
        /// </summary>
        [Column("is_preliminary", TypeName = "boolean")]
        public bool IsPreliminary { get; set; } = false;
    }
}
