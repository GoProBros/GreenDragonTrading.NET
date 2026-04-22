namespace GreenDragonTrading.Domain.Constants
{
    public static class MarketIndexConstant
    {
        // ── HOSE (HSX) broad market indices ─────────────────────────────────────

        /// <summary>Main composite index for all stocks listed on HOSE.</summary>
        public const string VNINDEX = "VNINDEX";

        /// <summary>Top 30 large-cap, high-liquidity stocks on HOSE.</summary>
        public const string VN30 = "VN30";

        /// <summary>Top 100 large-cap, high-liquidity stocks on HOSE.</summary>
        public const string VN100 = "VN100";

        /// <summary>Diamond index — large-cap stocks with significant foreign ownership on HOSE.</summary>
        public const string VNDIAMOND = "VNDIAMOND";

        /// <summary>Sustainability index — ESG-screened stocks on HOSE.</summary>
        public const string VNSI = "VNSI";

        // ── HNX indices ─────────────────────────────────────────────────────────

        /// <summary>Main composite index for all stocks listed on HNX.</summary>
        public const string HNXINDEX = "HNXINDEX";

        /// <summary>Top 30 large-cap, high-liquidity stocks on HNX.</summary>
        public const string HNX30 = "HNX30";
    }
}
