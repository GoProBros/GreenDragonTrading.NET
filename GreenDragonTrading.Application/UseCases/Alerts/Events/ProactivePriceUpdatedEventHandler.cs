using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.Alerts.Events
{
    /// <summary>
    /// Proactive producer flow:
    /// 1) pass Layer A and Layer B checks,
    /// 2) enqueue one AI evaluation job per ticker signal,
    /// 3) let background worker call AI once and fan-out to users.
    ///
    /// IMPORTANT: This handler runs in the hot path of market data streaming.
    /// All heavy processing is offloaded to a background task to avoid blocking
    /// real-time price updates. The handler itself returns immediately.
    /// </summary>
    public class ProactivePriceUpdatedEventHandler(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ProactivePriceUpdatedEventHandler> logger) : INotificationHandler<PriceUpdatedEvent>
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> TickerLocks = new();
        private static readonly SemaphoreSlim WatchListIndexBuildLock = new(1, 1);
        private static readonly TimeSpan WatchListUserCacheTtl = TimeSpan.FromMinutes(3);
        private static readonly TimeSpan LayerBSettingsCacheTtl = TimeSpan.FromMinutes(5);

        private static readonly HashSet<string> TradingStatusesAllowed = new(StringComparer.OrdinalIgnoreCase)
        {
            "N",
            "NL",
            "ST",
        };

        private static readonly HashSet<string> TradingStatusesBlocked = new(StringComparer.OrdinalIgnoreCase)
        {
            "D",
            "H",
            "S",
            "ND",
            "SA",
            "SP",
        };

        private const decimal MaxAbsoluteMovePercent = 20m;

        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
        private readonly ILogger<ProactivePriceUpdatedEventHandler> _logger = logger;

        /// <summary>
        /// Handle price update event without blocking the realtime streaming pipeline.
        /// Heavy work is offloaded to a background task with its own DI scope.
        /// </summary>
        public Task Handle(PriceUpdatedEvent notification, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(notification.Ticker))
            {
                return Task.CompletedTask;
            }

            var ticker = notification.Ticker.ToUpperInvariant();
            var currentPrice = notification.CurrentPrice;
            var currentVolume = notification.CurrentVolume ?? 0m;
            var referencePrice = notification.ReferencePrice;

            // Fire-and-forget: proactive alert processing must NOT block realtime market data.
            // A new DI scope is created so scoped services (Redis, EF Core) stay alive.
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var innerLogger = scope.ServiceProvider.GetRequiredService<ILogger<ProactivePriceUpdatedEventHandler>>();
                    await ProcessSignalAsync(scope.ServiceProvider, ticker, currentPrice, currentVolume, referencePrice);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Background proactive signal evaluation failed for {Ticker}", ticker);
                }
            });

            return Task.CompletedTask;
        }

        private static async Task ProcessSignalAsync(
            IServiceProvider serviceProvider,
            string ticker,
            decimal currentPrice,
            decimal currentVolume,
            decimal? referencePrice)
        {
            var redis = serviceProvider.GetRequiredService<IRedisService>();
            var uow = serviceProvider.GetRequiredService<IUnitOfWork>();
            var evidence = serviceProvider.GetRequiredService<IProactiveAlertEvidenceService>();
            var logger = serviceProvider.GetRequiredService<ILogger<ProactivePriceUpdatedEventHandler>>();

            var tickerLock = TickerLocks.GetOrAdd(ticker, _ => new SemaphoreSlim(1, 1));
            await tickerLock.WaitAsync();

            var traceId = Guid.NewGuid().ToString("N");
            var candidateUserCount = 0;
            var eligibleUserCount = 0;

            try
            {
                var candidateUserIds = await GetCandidateUserIdsByTickerAsync(redis, uow, ticker);
                if (candidateUserIds.Count == 0)
                {
                    return;
                }

                candidateUserCount = candidateUserIds.Count;

                var layerBSettings = await GetLayerBSettingsAsync(redis, uow);
                var throttleWindow = ResolveThrottleWindow(layerBSettings);
                var cooldownWindow = ResolveCooldownWindow(layerBSettings);

                // Check throttle BEFORE creating trace — skip silently if throttled
                if (!await ShouldEvaluateProactiveSignalAsync(redis, ticker, throttleWindow))
                {
                    return;
                }

                // ── STEP: trigger ──
                await evidence.AppendStepAsync(traceId, ticker, "trigger", "pass", detail: new()
                {
                    ["price"] = currentPrice.ToString(CultureInfo.InvariantCulture),
                    ["volume"] = currentVolume.ToString(CultureInfo.InvariantCulture),
                    ["refPrice"] = (referencePrice?.ToString(CultureInfo.InvariantCulture)) ?? "null",
                });

                var currentPrice = notification.CurrentPrice;
                var currentVolume = notification.CurrentVolume ?? 0m;
                var referencePrice = notification.ReferencePrice;

                var layerAContext = await BuildLayerAContextAsync(
                    redis,
                    ticker,
                    currentPrice,
                    currentVolume,
                    referencePrice);

                if (layerAContext == null)
                {
                    var absoluteMove = referencePrice.HasValue && referencePrice.Value > 0
                        ? Math.Abs(((currentPrice - referencePrice.Value) / referencePrice.Value) * 100m)
                        : 0m;
                    await evidence.AppendStepAsync(traceId, ticker, "layerA_filter", "fail",
                        failReason: absoluteMove > MaxAbsoluteMovePercent
                            ? $"Absolute move {absoluteMove:F2}% exceeds max {MaxAbsoluteMovePercent}%"
                            : "Invalid price/volume data or trading state halted/closed",
                        detail: new()
                        {
                            ["absMovePercent"] = absoluteMove.ToString("F2", CultureInfo.InvariantCulture),
                            ["maxAllowed"] = MaxAbsoluteMovePercent.ToString(CultureInfo.InvariantCulture),
                        });
                    await evidence.FinalizeTraceAsync(traceId, "filter_failed_layerA", candidateUserCount, 0, 0);
                    return;
                }

                // ── STEP: layerA_filter pass ──
                await evidence.AppendStepAsync(traceId, ticker, "layerA_filter", "pass", detail: new()
                {
                    ["signedMovePercent"] = layerAContext.SignedMovePercent.ToString("F2", CultureInfo.InvariantCulture),
                    ["tradingStatus"] = layerAContext.TradingStatus ?? "unknown",
                    ["tradingSession"] = layerAContext.TradingSession ?? "unknown",
                    ["candidateUserCount"] = candidateUserCount.ToString(),
                });

                var signalContext = await BuildRealtimeSignalContextAsync(redis, ticker, layerAContext, layerBSettings);
                if (signalContext.Context == null)
                {
                    await evidence.AppendStepAsync(traceId, ticker, "layerB_filter", "fail",
                        failReason: signalContext.FailReason ?? "Technical indicators missing, stale, or below thresholds (ADX/volume/trend alignment)",
                        detail: signalContext.FailDetail);
                    await evidence.FinalizeTraceAsync(traceId, "filter_failed_layerB", candidateUserCount, 0, 0);
                    return;
                }

                // ── STEP: layerB_filter pass ──
                await evidence.AppendStepAsync(traceId, ticker, "layerB_filter", "pass", detail: new()
                {
                    ["requiredMovePercent"] = signalContext.Context.RequiredMovePercent.ToString("F2", CultureInfo.InvariantCulture),
                    ["volumeRatio"] = signalContext.Context.VolumeRatio.ToString("F2", CultureInfo.InvariantCulture),
                    ["minVolumeRatio"] = layerBSettings.MinVolumeRatio.ToString(CultureInfo.InvariantCulture),
                    ["adx14"] = signalContext.Context.Adx14.ToString("F1", CultureInfo.InvariantCulture),
                    ["minAdx"] = layerBSettings.MinAdx.ToString(CultureInfo.InvariantCulture),
                    ["ema20"] = signalContext.Context.Ema20.ToString("F2", CultureInfo.InvariantCulture),
                    ["ema50"] = signalContext.Context.Ema50.ToString("F2", CultureInfo.InvariantCulture),
                });

                var activeUsers = (await uow.Users.FindAsync(
                    x => candidateUserIds.Contains(x.Id) && x.Status == CommonStatus.Active)).ToList();

                if (activeUsers.Count == 0)
                {
                    await evidence.AppendStepAsync(traceId, ticker, "cooldown_filter", "fail",
                        failReason: "All candidate users are inactive/deleted");
                    await evidence.FinalizeTraceAsync(traceId, "filter_failed_no_active_users", candidateUserCount, 0, 0);
                    return;
                }

                var eligibleUserIds = new List<Guid>();
                foreach (var user in activeUsers)
                {
                    if (!await TryAcquireProactiveCooldownAsync(redis, user.Id, ticker, cooldownWindow))
                    {
                        continue;
                    }

                    eligibleUserIds.Add(user.Id);
                }

                eligibleUserCount = eligibleUserIds.Count;

                if (eligibleUserIds.Count == 0)
                {
                    await evidence.AppendStepAsync(traceId, ticker, "cooldown_filter", "fail",
                        failReason: "All users are in cooldown period (20 min per user per ticker)",
                        detail: new() { ["totalActiveUsers"] = activeUsers.Count.ToString() });
                    await evidence.FinalizeTraceAsync(traceId, "filter_failed_cooldown", candidateUserCount, 0, 0);
                    return;
                }

                // ── STEP: cooldown_filter pass ──
                await evidence.AppendStepAsync(traceId, ticker, "cooldown_filter", "pass", detail: new()
                {
                    ["eligibleUsers"] = eligibleUserCount.ToString(),
                    ["totalCandidates"] = candidateUserCount.ToString(),
                });

                var job = BuildAiEvaluationJob(
                    eligibleUserIds,
                    ticker,
                    layerAContext,
                    signalContext.Context);
                job.TraceId = traceId;

                await redis.ListRightPushAsync(
                    RedisConstants.ProactiveAiEvaluationQueue(),
                    job);

                // ── STEP: enqueued ──
                await evidence.AppendStepAsync(traceId, ticker, "enqueued", "success", detail: new()
                {
                    ["jobId"] = job.JobId,
                    ["targetUserCount"] = eligibleUserCount.ToString(),
                });

                logger.LogInformation(
                    "Enqueued proactive AI evaluation job for {Ticker} with {UserCount} target users (trace={TraceId})",
                    ticker,
                    eligibleUserIds.Count,
                    traceId);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error producing proactive AI jobs for {Ticker}", ticker);
                try
                {
                    await evidence.AppendStepAsync(traceId, ticker, "error", "error",
                        failReason: ex.Message, cancellationToken: CancellationToken.None);
                    await evidence.FinalizeTraceAsync(traceId, "error", candidateUserCount, eligibleUserCount, 0, CancellationToken.None);
                }
                catch (Exception innerEx)
                {
                    logger.LogWarning(innerEx, "Failed to record error evidence for trace {TraceId}", traceId);
                }
            }
            finally
            {
                tickerLock.Release();
            }
        }

        private static async Task<bool> ShouldEvaluateProactiveSignalAsync(IRedisService redis, string ticker, TimeSpan throttleWindow)
        {
            var evalKey = ProactiveTickerEvaluationKey(ticker);
            if (await redis.ExistsAsync(evalKey))
            {
                return false;
            }

            await redis.SetAsync(evalKey, true, throttleWindow);
            return true;
        }

        private static async Task<ProactiveAlertLayerBSettingsDto> GetLayerBSettingsAsync(IRedisService redis, IUnitOfWork uow)
        {
            var cacheKey = RedisConstants.ProactiveAlertLayerBSettings();
            var cached = await redis.GetAsync<ProactiveAlertLayerBSettingsDto>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            var settings = await uow.ProactiveAlertLayerBSettings.FirstOrDefaultAsync(
                _ => true);

            ProactiveAlertLayerBSettingsDto snapshot;
            if (settings == null)
            {
                var now = DateTimeOffset.UtcNow;
                snapshot = new ProactiveAlertLayerBSettingsDto
                {
                    Id = 0,
                    Timeframe = ProactiveAlertLayerBDefaults.Timeframe,
                    MinAbsoluteMovePercent = ProactiveAlertLayerBDefaults.MinAbsoluteMovePercent,
                    AtrMoveMultiplier = ProactiveAlertLayerBDefaults.AtrMoveMultiplier,
                    MinVolumeRatio = ProactiveAlertLayerBDefaults.MinVolumeRatio,
                    MinAdx = ProactiveAlertLayerBDefaults.MinAdx,
                    MaxIndicatorSnapshotAgeMinutes = ProactiveAlertLayerBDefaults.MaxIndicatorSnapshotAgeMinutes,
                    ThrottleSeconds = ProactiveAlertLayerBDefaults.ThrottleSeconds,
                    CooldownMinutes = ProactiveAlertLayerBDefaults.CooldownMinutes,
                    CreatedAt = now,
                    UpdatedAt = now
                };
            }
            else
            {
                snapshot = new ProactiveAlertLayerBSettingsDto
                {
                    Id = settings.Id,
                    Timeframe = settings.Timeframe,
                    MinAbsoluteMovePercent = settings.MinAbsoluteMovePercent,
                    AtrMoveMultiplier = settings.AtrMoveMultiplier,
                    MinVolumeRatio = settings.MinVolumeRatio,
                    MinAdx = settings.MinAdx,
                    MaxIndicatorSnapshotAgeMinutes = settings.MaxIndicatorSnapshotAgeMinutes,
                    ThrottleSeconds = settings.ThrottleSeconds,
                    CooldownMinutes = settings.CooldownMinutes,
                    CreatedAt = settings.CreatedAt,
                    UpdatedAt = settings.UpdatedAt
                };
            }

            await redis.SetAsync(cacheKey, snapshot, LayerBSettingsCacheTtl);
            return snapshot;
        }

        private static async Task<LayerAContext?> BuildLayerAContextAsync(
            IRedisService redis,
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

            var marketData = await redis.GetHashAsync<MarketSymbolDto>(
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
            var status = NormalizeTradingStatus(tradingStatus);
            if (TradingStatusesBlocked.Contains(status))
            {
                return false;
            }

            if (TradingStatusesAllowed.Contains(status))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(tradingSession))
            {
                var session = tradingSession.Trim().ToLowerInvariant();
                if (session.Contains("halt") || session.Contains("suspend") || session.Contains("close") || session.Contains("break"))
                {
                    return false;
                }
            }

            return true;
        }

        private static string NormalizeTradingStatus(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var trimmed = value.Trim();
            var token = trimmed.Split([' ', '-', '_', '/', '\\', ':'], StringSplitOptions.RemoveEmptyEntries)[0];
            return token.ToUpperInvariant();
        }

        private static async Task<LayerBResult> BuildRealtimeSignalContextAsync(
            IRedisService redis,
            string ticker,
            LayerAContext layerAContext,
            ProactiveAlertLayerBSettingsDto settings)
        {
            var timeframe = string.IsNullOrWhiteSpace(settings.Timeframe)
                ? ProactiveAlertLayerBDefaults.Timeframe
                : settings.Timeframe;
            var maxSnapshotAgeMinutes = settings.MaxIndicatorSnapshotAgeMinutes > 0
                ? settings.MaxIndicatorSnapshotAgeMinutes
                : ProactiveAlertLayerBDefaults.MaxIndicatorSnapshotAgeMinutes;
            var maxSnapshotAge = TimeSpan.FromMinutes(maxSnapshotAgeMinutes);

            var snapshot = await redis.GetHashAsync<IndicatorSnapshotDto>(
                RedisConstants.Indicators(ticker, timeframe));

            if (snapshot == null)
            {
                return LayerBResult.Fail("Indicator snapshot not found in Redis.",
                    new() { ["indicatorKey"] = RedisConstants.Indicators(ticker, timeframe) });
            }

            if (!snapshot.Atr14.HasValue)
            {
                return LayerBResult.Fail("ATR14 indicator is missing.");
            }

            if (!snapshot.Adx14.HasValue)
            {
                return LayerBResult.Fail("ADX14 indicator is missing.");
            }

            if (!snapshot.Ema20.HasValue)
            {
                return LayerBResult.Fail("EMA20 indicator is missing.");
            }

            if (!snapshot.Ema50.HasValue)
            {
                return LayerBResult.Fail("EMA50 indicator is missing.");
            }

            if (snapshot.CalculatedAt == default)
            {
                return LayerBResult.Fail("Indicator snapshot has no calculation timestamp.");
            }

            var snapshotAge = DateTime.UtcNow - snapshot.CalculatedAt;
            if (snapshotAge > maxSnapshotAge)
            {
                return LayerBResult.Fail(
                    $"Indicator snapshot is stale: {snapshotAge.TotalHours:F1}h old (max allowed: {maxSnapshotAgeMinutes / 60.0:F1}h).",
                    new()
                    {
                        ["calculatedAt"] = snapshot.CalculatedAt.ToString("O"),
                        ["snapshotAgeHours"] = snapshotAge.TotalHours.ToString("F1"),
                        ["maxAgeHours"] = (maxSnapshotAgeMinutes / 60.0).ToString("F1"),
                    });
            }

            var absoluteMovePercent = Math.Abs(layerAContext.SignedMovePercent);
            var atrPercent = snapshot.Atr14.Value > 0
                ? (snapshot.Atr14.Value / layerAContext.CurrentPrice) * 100m
                : 0m;
            var requiredMovePercent = Math.Max(settings.MinAbsoluteMovePercent, settings.AtrMoveMultiplier * atrPercent);

            var volumeRatio = snapshot.VolumeToVolumeMa20Ratio ?? 0m;
            var adx = snapshot.Adx14.Value;
            var ema20 = snapshot.Ema20.Value;
            var ema50 = snapshot.Ema50.Value;

            var trendAligned = (ema20 > ema50 && layerAContext.CurrentPrice > ema20)
                || (ema20 < ema50 && layerAContext.CurrentPrice < ema20);

            var failReasons = new List<string>();
            var failDetail = new Dictionary<string, object?>();

            failDetail["absoluteMovePercent"] = absoluteMovePercent.ToString("F2", CultureInfo.InvariantCulture);
            failDetail["requiredMovePercent"] = requiredMovePercent.ToString("F2", CultureInfo.InvariantCulture);
            failDetail["atrPercent"] = atrPercent.ToString("F2", CultureInfo.InvariantCulture);
            failDetail["volumeRatio"] = volumeRatio.ToString("F2", CultureInfo.InvariantCulture);
            failDetail["minVolumeRatio"] = settings.MinVolumeRatio.ToString(CultureInfo.InvariantCulture);
            failDetail["adx14"] = adx.ToString("F1", CultureInfo.InvariantCulture);
            failDetail["minAdx"] = settings.MinAdx.ToString(CultureInfo.InvariantCulture);
            failDetail["ema20"] = ema20.ToString("F2", CultureInfo.InvariantCulture);
            failDetail["ema50"] = ema50.ToString("F2", CultureInfo.InvariantCulture);
            failDetail["currentPrice"] = layerAContext.CurrentPrice.ToString(CultureInfo.InvariantCulture);

            if (absoluteMovePercent < requiredMovePercent)
            {
                failReasons.Add($"Price move {absoluteMovePercent:F2}% below required {requiredMovePercent:F2}% (ATR%={atrPercent:F2}%, minAbs={settings.MinAbsoluteMovePercent}, multiplier={settings.AtrMoveMultiplier})");
            }

            if (volumeRatio < settings.MinVolumeRatio)
            {
                failReasons.Add($"Volume ratio {volumeRatio:F2} below minimum {settings.MinVolumeRatio}");
            }

            if (adx < settings.MinAdx)
            {
                failReasons.Add($"ADX {adx:F1} below minimum {settings.MinAdx}");
            }

            if (!trendAligned)
            {
                var trendDir = ema20 > ema50 ? "bullish" : "bearish";
                failReasons.Add($"Trend not aligned: EMA20 ({ema20:F2}) {(ema20 > ema50 ? ">" : "<")} EMA50 ({ema50:F2}), but price ({layerAContext.CurrentPrice}) not above EMA20 in bullish or not below EMA20 in bearish scenario");
            }

            if (failReasons.Count > 0)
            {
                return LayerBResult.Fail(string.Join(" | ", failReasons), failDetail);
            }

            return LayerBResult.Pass(new RealtimeSignalContext
            {
                RequiredMovePercent = requiredMovePercent,
                VolumeRatio = volumeRatio,
                Atr14 = snapshot.Atr14.Value,
                Adx14 = adx,
                Ema20 = ema20,
                Ema50 = ema50,
            });
        }

        private static async Task<List<Guid>> GetCandidateUserIdsByTickerAsync(IRedisService redis, IUnitOfWork uow, string ticker)
        {
            var watchListIndex = await GetWatchListTickerIndexAsync(redis, uow);
            if (!watchListIndex.TryGetValue(ticker, out var userIds) || userIds.Count == 0)
            {
                return new List<Guid>();
            }

            return userIds.Distinct().ToList();
        }

        private static async Task<Dictionary<string, List<Guid>>> GetWatchListTickerIndexAsync(IRedisService redis, IUnitOfWork uow)
        {
            var cacheKey = WatchListTickerIndexCacheKey();
            var cachedIndex = await redis.GetAsync<Dictionary<string, List<Guid>>>(cacheKey);
            if (cachedIndex != null)
            {
                return cachedIndex;
            }

            await WatchListIndexBuildLock.WaitAsync();
            try
            {
                cachedIndex = await redis.GetAsync<Dictionary<string, List<Guid>>>(cacheKey);
                if (cachedIndex != null)
                {
                    return cachedIndex;
                }

                var watchLists = await uow.WatchLists.FindAsync(
                    x => x.Status == CommonStatus.Active);

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

                await redis.SetAsync(cacheKey, normalizedIndex, WatchListUserCacheTtl);
                return normalizedIndex;
            }
            finally
            {
                WatchListIndexBuildLock.Release();
            }
        }

        private static async Task<bool> TryAcquireProactiveCooldownAsync(IRedisService redis, Guid userId, string ticker, TimeSpan cooldownWindow)
        {
            var cooldownKey = ProactiveUserCooldownKey(userId, ticker);
            if (await redis.ExistsAsync(cooldownKey))
            {
                return false;
            }

            await redis.SetAsync(cooldownKey, true, cooldownWindow);
            return true;
        }

        private static TimeSpan ResolveThrottleWindow(ProactiveAlertLayerBSettingsDto settings)
        {
            var seconds = settings.ThrottleSeconds > 0
                ? settings.ThrottleSeconds
                : ProactiveAlertLayerBDefaults.ThrottleSeconds;
            return TimeSpan.FromSeconds(seconds);
        }

        private static TimeSpan ResolveCooldownWindow(ProactiveAlertLayerBSettingsDto settings)
        {
            var minutes = settings.CooldownMinutes > 0
                ? settings.CooldownMinutes
                : ProactiveAlertLayerBDefaults.CooldownMinutes;
            return TimeSpan.FromMinutes(minutes);
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
            => RedisConstants.ProactiveWatchListTickerIndex();

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

        private sealed class LayerBResult
        {
            public RealtimeSignalContext? Context { get; private init; }
            public string? FailReason { get; private init; }
            public Dictionary<string, object?>? FailDetail { get; private init; }

            public static LayerBResult Pass(RealtimeSignalContext context)
                => new() { Context = context };

            public static LayerBResult Fail(string reason, Dictionary<string, object?>? detail = null)
                => new() { FailReason = reason, FailDetail = detail };
        }
    }
}
