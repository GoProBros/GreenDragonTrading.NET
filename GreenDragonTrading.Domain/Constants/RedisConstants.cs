namespace GreenDragonTrading.Domain.Constants
{
    public static class RedisConstants
    {
        /// <summary>
        /// Redis key prefix for market data entries.
        /// </summary>
        public const string REDIS_KEY_PREFIX_MARKET_DATA = "MarketData:Symbol";

        public const string REDIS_KEY_PREFIX_OHLCV = "OHLCV";
        public const string REDIS_KEY_PREFIX_HEATMAP = "HEATMAP";
        public const string REDIS_KEY_PREFIX_TRADES = "TRADES";
        public const string REDIS_KEY_PREFIX_INDICATORS = "INDICATORS";

        public const string SIGNALR_GROUP_PREFIX_OHLCV = "OHLCV";
        public const string SIGNALR_GROUP_PREFIX_TRADE = "TRADE";
        public const string SIGNALR_GROUP_PREFIX_DEPTH = "DEPTH";
        public const string HEATMAP_GROUP_ALL = "HEATMAP:ALL";

        public static string MarketDataSymbol(string ticker)
            => $"{REDIS_KEY_PREFIX_MARKET_DATA}:{ticker.ToUpperInvariant()}";

        public static string Ohlcv(string ticker, string timeframe)
            => $"{REDIS_KEY_PREFIX_OHLCV}:{ticker.ToUpperInvariant()}:{timeframe.ToUpperInvariant()}";

        public static string OhlcvPattern(string ticker)
            => $"{REDIS_KEY_PREFIX_OHLCV}:{ticker.ToUpperInvariant()}:*";

        public static string OhlcvSignalRGroup(string ticker, string timeframe)
            => $"{SIGNALR_GROUP_PREFIX_OHLCV}:{ticker.ToUpperInvariant()}:{timeframe.ToUpperInvariant()}";

        public static string Heatmap(string ticker)
            => $"{REDIS_KEY_PREFIX_HEATMAP}:{ticker.ToUpperInvariant()}";

        public static string HeatmapPattern()
            => $"{REDIS_KEY_PREFIX_HEATMAP}:*";

        public static string HeatmapExchangeGroup(string exchange)
            => $"{REDIS_KEY_PREFIX_HEATMAP}:{exchange.ToLowerInvariant()}";

        public static string HeatmapSectorGroup(string exchange, string sector)
            => $"{REDIS_KEY_PREFIX_HEATMAP}:{exchange.ToLowerInvariant()}:{sector}";

        public static string Trades(string ticker)
            => $"{REDIS_KEY_PREFIX_TRADES}:{ticker.ToUpperInvariant()}";

        public static string TradeSignalRGroup(string ticker)
            => $"{SIGNALR_GROUP_PREFIX_TRADE}:{ticker.ToUpperInvariant()}";

        public static string DepthSignalRGroup(string ticker)
            => $"{SIGNALR_GROUP_PREFIX_DEPTH}:{ticker.ToUpperInvariant()}";

        public static string Indicators(string ticker, string timeframe)
            => $"{REDIS_KEY_PREFIX_INDICATORS}:{ticker.ToUpperInvariant()}:{timeframe.ToUpperInvariant()}";
    }
}
