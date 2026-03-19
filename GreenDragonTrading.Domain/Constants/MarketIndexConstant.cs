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

        /// <summary>Mid-cap index — stocks ranked 31–100 on HOSE.</summary>
        public const string VNMID = "VNMID";

        /// <summary>Small-cap index — stocks ranked 101+ on HOSE.</summary>
        public const string VNSML = "VNSML";

        /// <summary>All-share index — all listed stocks on HOSE.</summary>
        public const string VNALL = "VNALL";

        /// <summary>Sustainability index — ESG-screened stocks on HOSE.</summary>
        public const string VNSI = "VNSI";

        /// <summary>Diamond index — large-cap stocks with significant foreign ownership on HOSE.</summary>
        public const string VNDIAMOND = "VNDIAMOND";

        // ── HOSE sector indices ─────────────────────────────────────────────────

        /// <summary>Consumer Discretionary sector index on HOSE.</summary>
        public const string VNCOND = "VNCOND";

        /// <summary>Consumer Staples sector index on HOSE.</summary>
        public const string VNCONS = "VNCONS";

        /// <summary>Energy sector index on HOSE.</summary>
        public const string VNENE = "VNENE";

        /// <summary>Financials sector index on HOSE.</summary>
        public const string VNFIN = "VNFIN";

        /// <summary>Financial Leaders index — top financial sector stocks on HOSE.</summary>
        public const string VNFINLEAD = "VNFINLEAD";

        /// <summary>Financial Select index — selected financial sector stocks on HOSE.</summary>
        public const string VNFINSELECT = "VNFINSELECT";

        /// <summary>Healthcare sector index on HOSE.</summary>
        public const string VNHEAL = "VNHEAL";

        /// <summary>Industrials sector index on HOSE.</summary>
        public const string VNIND = "VNIND";

        /// <summary>Information Technology sector index on HOSE.</summary>
        public const string VNIT = "VNIT";

        /// <summary>Materials sector index on HOSE.</summary>
        public const string VNMAT = "VNMAT";

        /// <summary>Real Estate sector index on HOSE.</summary>
        public const string VNREAL = "VNREAL";

        /// <summary>Utilities sector index on HOSE.</summary>
        public const string VNUTI = "VNUTI";

        // ── HOSE cross-market indices ───────────────────────────────────────────

        /// <summary>VNX50 index — top 50 stocks across HOSE and HNX combined.</summary>
        public const string VNX50 = "VNX50";

        /// <summary>VNX All-Share index — all stocks across HOSE and HNX combined.</summary>
        public const string VNXALL = "VNXALL";

        // ── HNX indices ─────────────────────────────────────────────────────────

        /// <summary>Main composite index for all stocks listed on HNX.</summary>
        public const string HNXINDEX = "HNXINDEX";

        /// <summary>Top 30 large-cap, high-liquidity stocks on HNX.</summary>
        public const string HNX30 = "HNX30";

        // ── UPCOM indices ────────────────────────────────────────────────────────

        /// <summary>Main composite index for all stocks listed on UPCOM.</summary>
        public const string HNXUPCOMINDEX = "HNXUPCOMINDEX";
    }
}
