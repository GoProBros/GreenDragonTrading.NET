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
        public const string REDIS_KEY_PROACTIVE_AI_EVALUATION_QUEUE = "alerts:proactive:ai:queue";
        public const string REDIS_KEY_PROACTIVE_WATCHLIST_INDEX = "alerts:proactive:watchlist-index";
        public const string REDIS_KEY_PROACTIVE_LAYER_B_SETTINGS = "alerts:proactive:layer-b-settings";
        public const string REDIS_KEY_PREFIX_PAYMENT_SYNC = "PAYMENT:SYNC";
        public const string REDIS_KEY_PENDING_PAYMENT_SYNC_QUEUE = "PAYMENT:SYNC:PENDING";
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

        public static string HeatmapResponse(string? exchange, string? sector)
            => $"{REDIS_KEY_PREFIX_HEATMAP}:RESPONSE:{exchange?.ToUpperInvariant() ?? "ALL"}:{sector ?? "ALL"}";

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

        public static string ProactiveAiEvaluationQueue()
            => REDIS_KEY_PROACTIVE_AI_EVALUATION_QUEUE;

        public static string ProactiveAlertLayerBSettings()
            => REDIS_KEY_PROACTIVE_LAYER_B_SETTINGS;

        // ── Proactive Alert Evidence ────────────────────────────────────────────
        private const string REDIS_KEY_PREFIX_EVIDENCE_TRACE = "evidence:trace";
        private const string REDIS_KEY_EVIDENCE_TIMELINE = "evidence:timeline";
        private const string REDIS_KEY_PREFIX_EVIDENCE_TICKER_INDEX = "evidence:ticker";
        private static readonly TimeSpan EvidenceDefaultTtl = TimeSpan.FromDays(7);

        public static string EvidenceTrace(string traceId)
            => $"{REDIS_KEY_PREFIX_EVIDENCE_TRACE}:{traceId}";

        public static string EvidenceTimeline()
            => REDIS_KEY_EVIDENCE_TIMELINE;

        public static string EvidenceTickerIndex(string ticker)
            => $"{REDIS_KEY_PREFIX_EVIDENCE_TICKER_INDEX}:{ticker.ToUpperInvariant()}";

        public static TimeSpan EvidenceTtl()
            => EvidenceDefaultTtl;

        public static string ProactiveWatchListTickerIndex()
            => REDIS_KEY_PROACTIVE_WATCHLIST_INDEX;

        public static string PendingPaymentSyncQueue()
            => REDIS_KEY_PENDING_PAYMENT_SYNC_QUEUE;

        public static string AlertsAbove(string ticker)
            => $"{REDIS_KEY_PREFIX_ALERTS_ABOVE}:{ticker.ToUpperInvariant()}";

        public static string AlertsBelow(string ticker)
            => $"{REDIS_KEY_PREFIX_ALERTS_BELOW}:{ticker.ToUpperInvariant()}";

        public static string AlertsByTypeAndCondition(string ticker, AlertType type, ConditionType condition)
            => $"alerts:{ticker.ToUpperInvariant()}:{(short)type}:{(short)condition}";

        public static string TelegramStartToken(string token)
            => $"telegram:start-token:{token}";

        /// <summary>Redis hash key for a live market index snapshot.</summary>
        public static string IndexData(string code)
            => $"{REDIS_KEY_PREFIX_INDEX_DATA}:{code.ToUpperInvariant()}";

        /// <summary>Redis list key for intraday price history of a market index.</summary>
        public static string IndexIntraday(string code)
            => $"{REDIS_KEY_PREFIX_INDEX_DATA}:INTRADAY:{code.ToUpperInvariant()}";

        /// <summary>Wildcard pattern matching all intraday list keys and their DATE sentinel keys.</summary>
        public static string IndexIntradayPattern()
            => $"{REDIS_KEY_PREFIX_INDEX_DATA}:INTRADAY:*";

        /// <summary>SignalR group name clients subscribe to for live index broadcasts.</summary>
        public static string IndexSignalRGroup(string code)
            => $"{SIGNALR_GROUP_PREFIX_INDEX}:{code.ToUpperInvariant()}";
    }
}
