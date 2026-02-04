using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Domain.Entities
{
    /// <summary>
    /// Mã chứng khoán
    /// </summary>
    [Table("symbols")]
    public class Symbol
    {
        [Key]
        [Column("ticker", TypeName = "varchar(20)")]
        [MaxLength(20)]
        public string Ticker { get; set; } = null!;

        [Column("isin", TypeName = "varchar(25)")]
        public string? Isin { get; set; }

        [Column("en_company_name", TypeName = "varchar(255)")]
        [MaxLength(255)]
        public string? EnCompanyName { get; set; } = null!;

        [Column("vi_company_name", TypeName = "varchar(255)")]
        [MaxLength(255)]
        public string? ViCompanyName { get; set; }

        [Column("exchange_code", TypeName = "varchar(20)")]
        [Required]
        [MaxLength(20)]
        public string ExchangeCode { get; set; } = null!;

        [Column("sector_id", TypeName = "varchar(10)")]
        public string? SectorId { get; set; } 

        /// <summary>
        /// Company logo file path
        /// </summary>
        [Column("logo_path", TypeName = "varchar(500)")]
        [MaxLength(500)]
        public string? LogoPath { get; set; }

        /// <summary>
        /// Phân loại: Cổ phiếu / ETF / Trái phiếu,...
        /// </summary>
        [Column("type")]
        [Required]
        public SymbolType Type { get; set; }

        [Column("trading_status", TypeName = "smallint")]
        public SymbolStatus TradingStatus { get; set; } = SymbolStatus.Normal;

        [Column("status", TypeName = "smallint")]
        [Required]
        public CommonStatus Status { get; set; } = CommonStatus.Active;

        // Navigation Properties
        [ForeignKey("ExchangeCode")]
        public virtual Exchange Exchange { get; set; } = null!;

        [ForeignKey("SectorId")]
        public virtual Sector? Sector { get; set; }
    }
}
