using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities;

/// <summary>
/// Data macro (kinh tế vĩ mô) theo thời gian
/// </summary>
[Table("macroeconomic_data")]
public class MacroeconomicData
{
    /// <summary>
    /// ID
    /// </summary>
    [Key]
    [Column("id", TypeName = "uuid")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Ngày ghi nhận dữ liệu
    /// </summary>
    [Column("record_date", TypeName = "date")]
    public DateOnly RecordDate { get; set; }

    /// <summary>
    /// Government Bonds Last Month Average Return
    /// </summary>
    [Column("gov_bonds_return", TypeName = "decimal(18,6)")]
    public decimal GovBondsReturn { get; set; }

    /// <summary>
    /// The value of the U.S. dollar (USD) in VND
    /// </summary>
    [Column("usd_vnd_exchange_rate", TypeName = "decimal(18,6)")]
    public decimal UsdVndExchangeRate { get; set; }

    /// <summary>
    /// The USD/VND last month return
    /// </summary>
    [Column("usd_vnd_exchange_rate_return", TypeName = "decimal(18,6)")]
    public decimal UsdVndExchangeRateReturn { get; set; }

    /// <summary>
    /// Equal-Weight Index last month return
    /// </summary>
    [Column("equal_weight_index_return", TypeName = "decimal(18,6)")]
    public decimal EqualWeightIndexReturn { get; set; }

    /// <summary>
    /// The market capitalization weighted index of TSE (or VNIndex)
    /// </summary>
    [Column("market_index_value", TypeName = "decimal(18,6)")]
    public decimal MarketIndexValue { get; set; }

    /// <summary>
    /// Weighted index three months return
    /// </summary>
    [Column("market_index_return", TypeName = "decimal(18,6)")]
    public decimal MarketIndexReturn { get; set; }

    /// <summary>
    /// 1 month return of Gold Spot USD
    /// </summary>
    [Column("gold_spot_usd_return", TypeName = "decimal(18,6)")]
    public decimal GoldSpotUsdReturn { get; set; }

    /// <summary>
    /// Thời điểm tạo record
    /// </summary>
    [Column("created_at", TypeName = "timestamp with time zone")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Thời điểm cập nhật cuối
    /// </summary>
    [Column("updated_at", TypeName = "timestamp with time zone")]
    public DateTimeOffset? UpdatedAt { get; set; }
}