using GreenDragonTrading.Domain.Enums;

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
        public const string REDIS_KEY_PREFIX_INDICATORS_ZSCORE = "INDICATORS:ZSCORE";
        public const string REDIS_KEY_PREFIX_ALERTS_ABOVE = "alerts:above";
        public const string REDIS_KEY_PREFIX_ALERTS_BELOW = "alerts:below";

        public const string SIGNALR_GROUP_PREFIX_OHLCV = "OHLCV";
        public const string SIGNALR_GROUP_PREFIX_TRADE = "TRADE";
        public const string SIGNALR_GROUP_PREFIX_DEPTH = "DEPTH";
        public const string HEATMAP_GROUP_ALL = "HEATMAP:ALL";

        public const string REDIS_KEY_PREFIX_INDEX_DATA = "INDEX";
        public const string SIGNALR_GROUP_PREFIX_INDEX = "INDEX";

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

        public static string IndicatorsZScore(string ticker, string timeframe)
            => $"{REDIS_KEY_PREFIX_INDICATORS_ZSCORE}:{ticker.ToUpperInvariant()}:{timeframe.ToUpperInvariant()}";

        public static string AlertsAbove(string ticker)
            => $"{REDIS_KEY_PREFIX_ALERTS_ABOVE}:{ticker.ToUpperInvariant()}";

        public static string AlertsBelow(string ticker)
            => $"{REDIS_KEY_PREFIX_ALERTS_BELOW}:{ticker.ToUpperInvariant()}";

        public static string AlertsByTypeAndCondition(string ticker, AlertType type, ConditionType condition)
            => $"alerts:{ticker.ToUpperInvariant()}:{(short)type}:{(short)condition}";
        /// <summary>Redis hash key for a live market index snapshot.</summary>
        public static string IndexData(string code)
            => $"{REDIS_KEY_PREFIX_INDEX_DATA}:{code.ToUpperInvariant()}";

        /// <summary>Redis list key for intraday price history of a market index.</summary>
        public static string IndexIntraday(string code)
            => $"{REDIS_KEY_PREFIX_INDEX_DATA}:INTRADAY:{code.ToUpperInvariant()}";

        /// <summary>SignalR group name clients subscribe to for live index broadcasts.</summary>
        public static string IndexSignalRGroup(string code)
            => $"{SIGNALR_GROUP_PREFIX_INDEX}:{code.ToUpperInvariant()}";
    }
}
