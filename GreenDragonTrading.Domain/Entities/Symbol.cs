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
        [Column("ticker", TypeName = "varchar(10)")]
        [MaxLength(10)]
        public string Ticker { get; set; } = null!;

        [Column("isin")]
        public string? Isin { get; set; }

        [Column("en_company_name", TypeName = "varchar(255)")]
        [Required]
        [MaxLength(255)]
        public string EnCompanyName { get; set; } = null!;

        [Column("vi_company_name", TypeName = "varchar(255)")]
        [MaxLength(255)]
        public string? ViCompanyName { get; set; }

        [Column("exchange_code", TypeName = "varchar(20)")]
        [Required]
        [MaxLength(20)]
        public string ExchangeCode { get; set; } = null!;

        [Column("sector_id")]
        [Required]
        public string? SectorId { get; set; } 

        /// <summary>
        /// Phân loại: Cổ phiếu / ETF / Trái phiếu,...
        /// </summary>
        [Column("type")]
        [Required]
        public SymbolType Type { get; set; }

        [Column("founding_date", TypeName = "date")]
        public DateTime? FoundingDate { get; set; }

        [Column("listing_date", TypeName = "date")]
        public DateTime? ListingDate { get; set; }

        [Column("charter_capital")]
        public long? CharterCapital { get; set; }

        [Column("first_price")]
        public long? FirstPrice { get; set; }

        [Column("address", TypeName = "varchar(255)")]
        [MaxLength(255)]
        public string? Address { get; set; }

        [Column("website", TypeName = "varchar(100)")]
        [MaxLength(100)]
        public string? Website { get; set; }

        [Column("telephone", TypeName = "varchar(25)")]
        [MaxLength(25)]
        public string? Telephone { get; set; }

        [Column("email", TypeName = "varchar(100)")]
        [MaxLength(100)]
        public string? Email { get; set; }

        [Column("fax", TypeName = "varchar(25)")]
        [MaxLength(25)]
        public string? Fax { get; set; }

        [Column("status", TypeName = "smallint")]
        [Required]
        public CommonStatus Status { get; set; } = CommonStatus.Active;

        // Navigation Properties
        [ForeignKey("ExchangeCode")]
        public virtual Exchange Exchange { get; set; } = null!;

        [ForeignKey("SectorId")]
        public virtual Sector Sector { get; set; } = null!;
    }
}
