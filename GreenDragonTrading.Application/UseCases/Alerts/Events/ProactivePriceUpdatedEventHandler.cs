using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.Alerts.Events
{
    /// <summary>
    /// Proactive producer flow:
    /// 1) pass Layer A and Layer B checks,
    /// 2) enqueue one AI evaluation job per ticker signal,
    /// 3) let background worker call AI once and fan-out to users.
    /// </summary>
    public class ProactivePriceUpdatedEventHandler(
        IRedisService redisService,
        IUnitOfWork uow,
        ILogger<ProactivePriceUpdatedEventHandler> logger) : INotificationHandler<PriceUpdatedEvent>
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> TickerLocks = new();
        private static readonly SemaphoreSlim WatchListIndexBuildLock = new(1, 1);
        private static readonly TimeSpan ProactiveTickerEvaluationThrottle = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan ProactiveUserCooldown = TimeSpan.FromMinutes(20);
        private static readonly TimeSpan WatchListUserCacheTtl = TimeSpan.FromMinutes(3);
        private static readonly TimeSpan MaxIndicatorSnapshotAge = TimeSpan.FromDays(2);

        private const decimal MinAbsoluteMovePercent = 1.0m;
        private const decimal MaxAbsoluteMovePercent = 20m;
        private const decimal AtrMoveMultiplier = 0.7m;
        private const decimal MinVolumeRatio = 1.8m;
        private const decimal MinAdx = 18m;
        private const string ProactiveTimeframe = "D1";

        private readonly IRedisService _redisService = redisService;
        private readonly IUnitOfWork _uow = uow;
        private readonly ILogger<ProactivePriceUpdatedEventHandler> _logger = logger;

        public async Task Handle(PriceUpdatedEvent notification, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(notification.Ticker))
            {
                return;
            }

            var ticker = notification.Ticker.ToUpperInvariant();
            var tickerLock = TickerLocks.GetOrAdd(ticker, _ => new SemaphoreSlim(1, 1));
            await tickerLock.WaitAsync(cancellationToken);

            try
            {
                if (!await ShouldEvaluateProactiveSignalAsync(ticker))
                {
                    return;
                }

                var candidateUserIds = await GetCandidateUserIdsByTickerAsync(ticker, cancellationToken);
                if (candidateUserIds.Count == 0)
                {
                    return;
                }

                var currentPrice = notification.CurrentPrice;
                var currentVolume = notification.CurrentVolume ?? 0m;
                var referencePrice = notification.ReferencePrice;

                var layerAContext = await BuildLayerAContextAsync(
                    ticker,
                    currentPrice,
                    currentVolume,
                    referencePrice);

                if (layerAContext == null)
                {
                    return;
                }

                var signalContext = await BuildRealtimeSignalContextAsync(ticker, layerAContext);
                if (signalContext == null)
                {
                    return;
                }

                var activeUsers = (await _uow.Users.FindAsync(
                    x => candidateUserIds.Contains(x.Id) && x.Status == CommonStatus.Active,
                    cancellationToken)).ToList();

                if (activeUsers.Count == 0)
                {
                    return;
                }

                var eligibleUserIds = new List<Guid>();
                foreach (var user in activeUsers)
                {
                    if (!await TryAcquireProactiveCooldownAsync(user.Id, ticker))
                    {
                        continue;
                    }

                    eligibleUserIds.Add(user.Id);
                }

                if (eligibleUserIds.Count == 0)
                {
                    return;
                }

                var job = BuildAiEvaluationJob(
                    eligibleUserIds,
                    ticker,
                    layerAContext,
                    signalContext);

                await _redisService.ListRightPushAsync(
                    RedisConstants.ProactiveAiEvaluationQueue(),
                    job);

                _logger.LogInformation(
                    "Enqueued proactive AI evaluation job for {Ticker} with {UserCount} target users",
                    ticker,
                    eligibleUserIds.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error producing proactive AI jobs for {Ticker}", ticker);
            }
            finally
            {
                tickerLock.Release();
            }
        }

        private async Task<bool> ShouldEvaluateProactiveSignalAsync(string ticker)
        {
            var evalKey = ProactiveTickerEvaluationKey(ticker);
            if (await _redisService.ExistsAsync(evalKey))
            {
                return false;
            }

            await _redisService.SetAsync(evalKey, true, ProactiveTickerEvaluationThrottle);
            return true;
        }

        private async Task<LayerAContext?> BuildLayerAContextAsync(
            string ticker,
            decimal currentPrice,
            decimal currentVolume,
            decimal? referencePrice)
        {
            if (!referencePrice.HasValue || referencePrice.Value <= 0 || currentPrice <= 0 || currentVolume <= 0)
            {
                return null;
            }

            var signedMovePercent = ((currentPrice - referencePrice.Value) / referencePrice.Value) * 100m;
            var absoluteMovePercent = Math.Abs(signedMovePercent);
            if (absoluteMovePercent > MaxAbsoluteMovePercent)
            {
                return null;
            }

            var marketData = await _redisService.GetHashAsync<MarketSymbolDto>(
                RedisConstants.MarketDataSymbol(ticker));

            if (marketData == null || marketData.LastPrice <= 0 || marketData.TotalVol < 0)
            {
                return null;
            }

            if (!IsTradingStateEligible(marketData.TradingStatus, marketData.TradingSession))
            {
                return null;
            }

            return new LayerAContext
            {
                ReferencePrice = referencePrice.Value,
                CurrentPrice = currentPrice,
                CurrentVolume = currentVolume,
                SignedMovePercent = signedMovePercent,
                TradingStatus = marketData.TradingStatus,
                TradingSession = marketData.TradingSession,
            };
        }

        private static bool IsTradingStateEligible(string? tradingStatus, string? tradingSession)
        {
            if (!string.IsNullOrWhiteSpace(tradingStatus))
            {
                var status = tradingStatus.Trim().ToLowerInvariant();
                if (status.Contains("halt") || status.Contains("suspend") || status.Contains("close"))
                {
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(tradingSession))
            {
                var session = tradingSession.Trim().ToLowerInvariant();
                if (session.Contains("closed") || session.Contains("break"))
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<RealtimeSignalContext?> BuildRealtimeSignalContextAsync(
            string ticker,
            LayerAContext layerAContext)
        {
            var snapshot = await _redisService.GetHashAsync<IndicatorSnapshotDto>(
                RedisConstants.Indicators(ticker, ProactiveTimeframe));

            if (snapshot == null
                || !snapshot.Atr14.HasValue
                || !snapshot.Adx14.HasValue
                || !snapshot.Ema20.HasValue
                || !snapshot.Ema50.HasValue
                || snapshot.CalculatedAt == default
                || DateTime.UtcNow - snapshot.CalculatedAt > MaxIndicatorSnapshotAge)
            {
                return null;
            }

            var absoluteMovePercent = Math.Abs(layerAContext.SignedMovePercent);
            var atrPercent = snapshot.Atr14.Value > 0
                ? (snapshot.Atr14.Value / layerAContext.CurrentPrice) * 100m
                : 0m;
            var requiredMovePercent = Math.Max(MinAbsoluteMovePercent, AtrMoveMultiplier * atrPercent);

            var volumeRatio = snapshot.VolumeToVolumeMa20Ratio ?? 0m;
            var adx = snapshot.Adx14.Value;
            var ema20 = snapshot.Ema20.Value;
            var ema50 = snapshot.Ema50.Value;

            var trendAligned = (ema20 > ema50 && layerAContext.CurrentPrice > ema20)
                || (ema20 < ema50 && layerAContext.CurrentPrice < ema20);

            if (absoluteMovePercent < requiredMovePercent
                || volumeRatio < MinVolumeRatio
                || adx < MinAdx
                || !trendAligned)
            {
                return null;
            }

            return new RealtimeSignalContext
            {
                RequiredMovePercent = requiredMovePercent,
                VolumeRatio = volumeRatio,
                Atr14 = snapshot.Atr14.Value,
                Adx14 = adx,
                Ema20 = ema20,
                Ema50 = ema50,
            };
        }

        private async Task<List<Guid>> GetCandidateUserIdsByTickerAsync(string ticker, CancellationToken cancellationToken)
        {
            var watchListIndex = await GetWatchListTickerIndexAsync(cancellationToken);
            if (!watchListIndex.TryGetValue(ticker, out var userIds) || userIds.Count == 0)
            {
                return new List<Guid>();
            }

            return userIds.Distinct().ToList();
        }

        private async Task<Dictionary<string, List<Guid>>> GetWatchListTickerIndexAsync(CancellationToken cancellationToken)
        {
            var cacheKey = WatchListTickerIndexCacheKey();
            var cachedIndex = await _redisService.GetAsync<Dictionary<string, List<Guid>>>(cacheKey);
            if (cachedIndex != null)
            {
                return cachedIndex;
            }

            await WatchListIndexBuildLock.WaitAsync(cancellationToken);
            try
            {
                cachedIndex = await _redisService.GetAsync<Dictionary<string, List<Guid>>>(cacheKey);
                if (cachedIndex != null)
                {
                    return cachedIndex;
                }

                var watchLists = await _uow.WatchLists.FindAsync(
                    x => x.Status == CommonStatus.Active,
                    cancellationToken);

                var index = new Dictionary<string, HashSet<Guid>>(StringComparer.OrdinalIgnoreCase);
                foreach (var watchList in watchLists)
                {
                    var tickers = ExtractTickersFromWatchList(watchList.Tickers);
                    foreach (var watchListTicker in tickers)
                    {
                        if (!index.TryGetValue(watchListTicker, out var users))
                        {
                            users = new HashSet<Guid>();
                            index[watchListTicker] = users;
                        }

                        users.Add(watchList.UserId);
                    }
                }

                var normalizedIndex = index.ToDictionary(
                    x => x.Key.ToUpperInvariant(),
                    x => x.Value.ToList(),
                    StringComparer.OrdinalIgnoreCase);

                await _redisService.SetAsync(cacheKey, normalizedIndex, WatchListUserCacheTtl);
                return normalizedIndex;
            }
            finally
            {
                WatchListIndexBuildLock.Release();
            }
        }

        private async Task<bool> TryAcquireProactiveCooldownAsync(Guid userId, string ticker)
        {
            var cooldownKey = ProactiveUserCooldownKey(userId, ticker);
            if (await _redisService.ExistsAsync(cooldownKey))
            {
                return false;
            }

            await _redisService.SetAsync(cooldownKey, true, ProactiveUserCooldown);
            return true;
        }

        private static List<string> ExtractTickersFromWatchList(string tickersJson)
        {
            if (string.IsNullOrWhiteSpace(tickersJson))
            {
                return new List<string>();
            }

            var tickers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using var document = JsonDocument.Parse(tickersJson);
                if (document.RootElement.ValueKind != JsonValueKind.Array)
                {
                    return new List<string>();
                }

                foreach (var element in document.RootElement.EnumerateArray())
                {
                    if (element.ValueKind == JsonValueKind.String
                        && !string.IsNullOrWhiteSpace(element.GetString()))
                    {
                        tickers.Add(element.GetString()!.ToUpperInvariant());
                        continue;
                    }

                    if (element.ValueKind == JsonValueKind.Object
                        && element.TryGetProperty("ticker", out var tickerElement)
                        && tickerElement.ValueKind == JsonValueKind.String
                        && !string.IsNullOrWhiteSpace(tickerElement.GetString()))
                    {
                        tickers.Add(tickerElement.GetString()!.ToUpperInvariant());
                    }
                }
            }
            catch (JsonException)
            {
                return new List<string>();
            }

            return tickers.ToList();
        }

        private static ProactiveAiEvaluationJobDto BuildAiEvaluationJob(
            List<Guid> targetUserIds,
            string ticker,
            LayerAContext layerAContext,
            RealtimeSignalContext signalContext)
        {
            return new ProactiveAiEvaluationJobDto
            {
                TargetUserIds = targetUserIds,
                Ticker = ticker,
                EnqueuedAt = DateTimeOffset.UtcNow,
                ReferencePrice = layerAContext.ReferencePrice,
                CurrentPrice = layerAContext.CurrentPrice,
                CurrentVolume = layerAContext.CurrentVolume,
                SignedMovePercent = layerAContext.SignedMovePercent,
                TradingStatus = layerAContext.TradingStatus,
                TradingSession = layerAContext.TradingSession,
                RequiredMovePercent = signalContext.RequiredMovePercent,
                VolumeRatio = signalContext.VolumeRatio,
                Atr14 = signalContext.Atr14,
                Adx14 = signalContext.Adx14,
                Ema20 = signalContext.Ema20,
                Ema50 = signalContext.Ema50,
            };
        }

        private static string ProactiveTickerEvaluationKey(string ticker)
            => $"alerts:proactive:eval:{ticker}";

        private static string WatchListTickerIndexCacheKey()
            => "alerts:proactive:watchlist-index";

        private static string ProactiveUserCooldownKey(Guid userId, string ticker)
            => $"alerts:proactive:cooldown:{userId:N}:{ticker}";

        private sealed class RealtimeSignalContext
        {
            public decimal RequiredMovePercent { get; init; }
            public decimal VolumeRatio { get; init; }
            public decimal Atr14 { get; init; }
            public decimal Adx14 { get; init; }
            public decimal Ema20 { get; init; }
            public decimal Ema50 { get; init; }
        }

        private sealed class LayerAContext
        {
            public decimal ReferencePrice { get; init; }
            public decimal CurrentPrice { get; init; }
            public decimal CurrentVolume { get; init; }
            public decimal SignedMovePercent { get; init; }
            public string? TradingStatus { get; init; }
            public string? TradingSession { get; init; }
        }
    }
}
