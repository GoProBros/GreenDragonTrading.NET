using System;

namespace GreenDragonTrading.Application.DTOs
{
    public class MacroeconomicDataDto
    {
        public DateOnly RecordDate { get; set; }
        public decimal GovBondsReturn { get; set; }
        public decimal UsdVndExchangeRate { get; set; }
        public decimal UsdVndExchangeRateReturn { get; set; }
        public decimal EqualWeightIndexReturn { get; set; }
        public decimal MarketIndexValue { get; set; }
        public decimal MarketIndexReturn { get; set; }
        public decimal GoldSpotUsdReturn { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}