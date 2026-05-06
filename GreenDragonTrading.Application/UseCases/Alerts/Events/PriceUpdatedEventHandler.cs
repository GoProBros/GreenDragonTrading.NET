using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.DTOs.Realtime;
using GreenDragonTrading.Application.Common.Utils;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using System.Collections.Concurrent;

namespace GreenDragonTrading.Application.UseCases.Alerts.Events
{
    public class PriceUpdatedEventHandler(
        IRedisService redisService,
        IOhlcvUnitOfWork ohlcvUow,
        IUnitOfWork uow,
        INotificationBroadcaster notificationBroadcaster,
        ITelegramBotService telegramBotService) : INotificationHandler<PriceUpdatedEvent>
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> TickerLocks = new();

        private readonly IRedisService _redisService = redisService;
        private readonly IOhlcvUnitOfWork _ohlcvUow = ohlcvUow;
        private readonly IUnitOfWork _uow = uow;
        private readonly INotificationBroadcaster _notificationBroadcaster = notificationBroadcaster;
        private readonly ITelegramBotService _telegramBotService = telegramBotService;

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
                var templateCache = new Dictionary<(AlertType, ConditionType), AlertTemplate?>();
                AlertTemplate? defaultTemplate = null;
                var defaultTemplateResolved = false;

                var currentPrice = notification.CurrentPrice;
                var currentVolume = notification.CurrentVolume ?? 0m;
                var referencePrice = notification.ReferencePrice;
                var previousVolume = notification.PreviousVolume;

                var pricePercentUp = 0m;
                var pricePercentDown = 0m;
                if (referencePrice.HasValue && referencePrice.Value > 0)
                {
                    var pct = ((currentPrice - referencePrice.Value) / referencePrice.Value) * 100m;
                    pricePercentUp = Math.Max(pct, 0m);
                    pricePercentDown = Math.Max(-pct, 0m);
                }

                var volumePercentUp = 0m;
                var volumePercentDown = 0m;
                if (previousVolume.HasValue && previousVolume.Value > 0)
                {
                    var volPct = ((currentVolume - previousVolume.Value) / previousVolume.Value) * 100m;
                    volumePercentUp = Math.Max(volPct, 0m);
                    volumePercentDown = Math.Max(-volPct, 0m);
                }

                var alertIds = new List<string>();

                alertIds.AddRange(await GetAlertIdsAsync(ticker, AlertType.Price, ConditionType.Above, double.NegativeInfinity, (double)currentPrice));
                alertIds.AddRange(await GetAlertIdsAsync(ticker, AlertType.Price, ConditionType.Below, (double)currentPrice, double.PositiveInfinity));

                alertIds.AddRange(await GetAlertIdsAsync(ticker, AlertType.Price, ConditionType.PercentChangeUp, double.NegativeInfinity, double.PositiveInfinity));
                alertIds.AddRange(await GetAlertIdsAsync(ticker, AlertType.Price, ConditionType.PercentChangeDown, double.NegativeInfinity, double.PositiveInfinity));

                alertIds.AddRange(await GetAlertIdsAsync(ticker, AlertType.Volume, ConditionType.Above, double.NegativeInfinity, double.PositiveInfinity));
                alertIds.AddRange(await GetAlertIdsAsync(ticker, AlertType.Volume, ConditionType.Below, double.NegativeInfinity, double.PositiveInfinity));
                alertIds.AddRange(await GetAlertIdsAsync(ticker, AlertType.Volume, ConditionType.PercentChangeUp, double.NegativeInfinity, double.PositiveInfinity));
                alertIds.AddRange(await GetAlertIdsAsync(ticker, AlertType.Volume, ConditionType.PercentChangeDown, double.NegativeInfinity, double.PositiveInfinity));

                var allAlertIds = alertIds
                    .Select(ParseAlertId)
                    .Where(x => x.HasValue)
                    .Select(x => x!.Value)
                    .Distinct()
                    .ToList();

                if (allAlertIds.Count == 0)
                {
                    return;
                }

                var alerts = await _uow.Alerts.GetActiveAlertsByIdsAsync(allAlertIds, cancellationToken);
                if (alerts.Count == 0)
                {
                    return;
                }

                var telegramChatIdsByUser = await GetTelegramChatIdsAsync(alerts, cancellationToken);

                var now = DateTimeOffset.UtcNow;
                var hasChanges = false;
                var systemSessionIdsByUser = new Dictionary<Guid, int>();
                var currentCandleCache = new Dictionary<string, CurrentCandleDto?>(StringComparer.OrdinalIgnoreCase);
                var closedCandleCache = new Dictionary<string, List<Domain.Entities.Ohlcv>>(StringComparer.OrdinalIgnoreCase);

                foreach (var alert in alerts)
                {
                    var shouldTrigger = false;
                    var effectiveCurrentVolume = currentVolume;
                    var effectiveVolumePercentUp = volumePercentUp;
                    var effectiveVolumePercentDown = volumePercentDown;

                    if (alert.Type == AlertType.Volume)
                    {
                        if (!alert.VolumeTimeFrame.HasValue)
                        {
                            continue;
                        }

                        var timeframe = ToTimeframeString(alert.VolumeTimeFrame.Value);
                        var useCurrentCandle = alert.Condition is ConditionType.Above or ConditionType.PercentChangeUp;
                        var snapshot = await GetVolumeSnapshotAsync(
                            ticker,
                            timeframe,
                            alert.VolumeLookbackBars,
                            useCurrentCandle,
                            currentCandleCache,
                            closedCandleCache,
                            cancellationToken);

                        if (snapshot == null)
                        {
                            continue;
                        }

                        effectiveCurrentVolume = snapshot.CurrentVolume;
                        effectiveVolumePercentUp = 0m;
                        effectiveVolumePercentDown = 0m;

                        if (alert.Condition == ConditionType.Above)
                        {
                            shouldTrigger = alert.ThresholdValue.HasValue
                                && effectiveCurrentVolume >= alert.ThresholdValue.Value;
                        }
                        else if (alert.Condition == ConditionType.Below)
                        {
                            shouldTrigger = alert.ThresholdValue.HasValue
                                && effectiveCurrentVolume <= alert.ThresholdValue.Value;
                        }
                        else if (alert.Condition is ConditionType.PercentChangeUp or ConditionType.PercentChangeDown)
                        {
                            if (!snapshot.BaselineMinVolume.HasValue || snapshot.BaselineMinVolume.Value <= 0)
                            {
                                continue;
                            }

                            if (!alert.ChangePercentage.HasValue)
                            {
                                continue;
                            }

                            var baseline = snapshot.BaselineMinVolume.Value;
                            var pct = ((effectiveCurrentVolume - baseline) / baseline) * 100m;
                            effectiveVolumePercentUp = Math.Max(pct, 0m);
                            effectiveVolumePercentDown = Math.Max(-pct, 0m);

                            shouldTrigger = alert.Condition == ConditionType.PercentChangeUp
                                ? pct >= alert.ChangePercentage.Value
                                : pct <= -alert.ChangePercentage.Value;
                        }
                    }
                    else
                    {
                        shouldTrigger = IsTriggered(
                            alert,
                            currentPrice,
                            currentVolume,
                            pricePercentUp,
                            pricePercentDown,
                            volumePercentUp,
                            volumePercentDown);
                    }

                    if (!shouldTrigger)
                    {
                        continue;
                    }

                    alert.IsTriggered = true;
                    alert.LastTriggeredAt = now;
                    alert.UpdatedAt = now;
                    _uow.Alerts.Update(alert);
                    hasChanges = true;

                    await _redisService.SortedSetRemoveAsync(
                        RedisConstants.AlertsByTypeAndCondition(alert.Ticker, alert.Type, alert.Condition),
                        alert.Id.ToString());

                    var template = await ResolveTemplateAsync(alert.Type, alert.Condition);
                    var message = AlertTemplateRenderingHelper.BuildAlertMessage(
                        alert,
                        currentPrice,
                        effectiveCurrentVolume,
                        pricePercentUp,
                        pricePercentDown,
                        effectiveVolumePercentUp,
                        effectiveVolumePercentDown,
                        template);

                    if (!systemSessionIdsByUser.TryGetValue(alert.UserId, out var systemSessionId))
                    {
                        systemSessionId = await GetOrCreateSystemSessionIdAsync(alert.UserId, now, cancellationToken);
                        systemSessionIdsByUser[alert.UserId] = systemSessionId;
                    }

                    var savedMessage = await SaveAlertChatMessageAsync(systemSessionId, message, now, cancellationToken);

                    await _notificationBroadcaster.BroadcastSystemChatMessageAsync(
                        alert.UserId,
                        new SystemChatMessageSignalREventDto
                        {
                            SessionId = systemSessionId,
                            MessageId = savedMessage.Id,
                            MessageType = savedMessage.MessageType.ToString(),
                            Source = "AlertTriggered",
                            Content = savedMessage.Content,
                            CreatedAt = savedMessage.CreatedAt,
                            AlertId = alert.Id,
                            Ticker = ticker,
                        },
                        cancellationToken);

                    await TrySendTelegramAsync(
                        telegramChatIdsByUser,
                        alert.UserId,
                        message,
                        cancellationToken);
                }

                if (hasChanges)
                {
                    await _uow.SaveChangesAsync(cancellationToken);
                }

                async Task<AlertTemplate?> ResolveTemplateAsync(AlertType type, ConditionType condition)
                {
                    if (!templateCache.TryGetValue((type, condition), out var cachedTemplate))
                    {
                        cachedTemplate = await _uow.AlertTemplates.GetActiveByTypeAndConditionAsync(
                            type,
                            condition,
                            cancellationToken);
                        templateCache[(type, condition)] = cachedTemplate;
                    }

                    if (cachedTemplate != null)
                    {
                        return cachedTemplate;
                    }

                    if (!defaultTemplateResolved)
                    {
                        defaultTemplate = await _uow.AlertTemplates.GetDefaultAsync(cancellationToken);
                        defaultTemplateResolved = true;
                    }

                    return defaultTemplate;
                }
            }
            finally
            {
                tickerLock.Release();
            }
        }

        private async Task<Dictionary<Guid, string>> GetTelegramChatIdsAsync(
            IReadOnlyCollection<Alert> alerts,
            CancellationToken cancellationToken)
        {
            var userIds = alerts
                .Select(x => x.UserId)
                .Distinct()
                .ToList();

            if (userIds.Count == 0)
            {
                return new Dictionary<Guid, string>();
            }

            var users = await _uow.Users.FindAsync(
                x => userIds.Contains(x.Id) && !string.IsNullOrEmpty(x.TelegramId),
                cancellationToken);

            return users
                .GroupBy(x => x.Id)
                .ToDictionary(
                    x => x.Key,
                    x => x.First().TelegramId!);
        }

        private async Task TrySendTelegramAsync(
            IReadOnlyDictionary<Guid, string> telegramChatIdsByUser,
            Guid userId,
            string message,
            CancellationToken cancellationToken)
        {
            if (!telegramChatIdsByUser.TryGetValue(userId, out var telegramChatId))
            {
                return;
            }

            await _telegramBotService.SendTextMessageAsync(
                telegramChatId,
                message,
                cancellationToken);
        }

        private async Task<List<string>> GetAlertIdsAsync(
            string ticker,
            AlertType alertType,
            ConditionType condition,
            double start,
            double stop)
        {
            var key = RedisConstants.AlertsByTypeAndCondition(ticker, alertType, condition);
            return await _redisService.SortedSetRangeByScoreAsync(key, start, stop);
        }

        private async Task<VolumeSnapshot?> GetVolumeSnapshotAsync(
            string ticker,
            string timeframe,
            int? lookbackBars,
            bool useCurrentCandle,
            Dictionary<string, CurrentCandleDto?> currentCandleCache,
            Dictionary<string, List<Domain.Entities.Ohlcv>> closedCandleCache,
            CancellationToken cancellationToken)
        {
            var extraClosed = useCurrentCandle ? 0 : 1;
            var requiredClosed = Math.Max((lookbackBars ?? 1) + extraClosed, 1);
            var closedCandles = await GetClosedCandlesAsync(
                ticker,
                timeframe,
                requiredClosed,
                closedCandleCache,
                cancellationToken);

            if (closedCandles.Count == 0)
            {
                return null;
            }

            var currentVolume = (decimal)closedCandles[0].Volume;

            if (useCurrentCandle)
            {
                var currentCandle = await GetCurrentCandleAsync(ticker, timeframe, currentCandleCache);
                if (currentCandle != null && currentCandle.Volume > 0)
                {
                    currentVolume = currentCandle.Volume;
                }
            }

            decimal? baselineMin = null;
            if (lookbackBars.HasValue && lookbackBars.Value > 0)
            {
                if (closedCandles.Count < lookbackBars.Value + extraClosed)
                {
                    return null;
                }

                var baselineCandles = useCurrentCandle
                    ? closedCandles.Take(lookbackBars.Value)
                    : closedCandles.Skip(1).Take(lookbackBars.Value);

                baselineMin = baselineCandles.Min(candle => (decimal)candle.Volume);
            }

            return new VolumeSnapshot(currentVolume, baselineMin);
        }

        private async Task<CurrentCandleDto?> GetCurrentCandleAsync(
            string ticker,
            string timeframe,
            Dictionary<string, CurrentCandleDto?> cache)
        {
            var cacheKey = $"{ticker}:{timeframe}:current";
            if (cache.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }

            var redisKey = RedisConstants.Ohlcv(ticker, timeframe);
            var candle = await _redisService.GetAsync<CurrentCandleDto>(redisKey);
            cache[cacheKey] = candle;
            return candle;
        }

        private async Task<List<Domain.Entities.Ohlcv>> GetClosedCandlesAsync(
            string ticker,
            string timeframe,
            int limit,
            Dictionary<string, List<Domain.Entities.Ohlcv>> cache,
            CancellationToken cancellationToken)
        {
            var cacheKey = $"{ticker}:{timeframe}:{limit}";
            if (cache.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }

            List<Domain.Entities.Ohlcv> candles;
            if (OhlcvConstants.Timeframes.IsStored(timeframe))
            {
                candles = await _ohlcvUow.Ohlcv.GetLatestCandlesAsync(
                    ticker,
                    timeframe,
                    limit,
                    cancellationToken);
            }
            else
            {
                candles = await GetComputedCandlesAsync(
                    ticker,
                    timeframe,
                    limit,
                    cancellationToken);
            }

            candles = candles
                .OrderByDescending(candle => candle.Time)
                .ToList();

            cache[cacheKey] = candles;
            return candles;
        }

        private async Task<List<Domain.Entities.Ohlcv>> GetComputedCandlesAsync(
            string ticker,
            string timeframe,
            int limit,
            CancellationToken cancellationToken)
        {
            if (!OhlcvConstants.Timeframes.ComputedFromM1.Contains(timeframe))
            {
                return new List<Domain.Entities.Ohlcv>();
            }

            var newestTime = await _ohlcvUow.Ohlcv.GetNewestDateAsync(
                ticker,
                OhlcvConstants.Timeframes.M1,
                cancellationToken);

            if (!newestTime.HasValue)
            {
                return new List<Domain.Entities.Ohlcv>();
            }

            var targetMinutes = OhlcvConstants.Timeframes.ToMinutes[timeframe];
            var lookbackMinutes = Math.Max(1, limit) * targetMinutes;
            var fromTime = newestTime.Value.AddMinutes(-(lookbackMinutes + targetMinutes * 2));

            var sourceData = await _ohlcvUow.Ohlcv.GetByTickerAndTimeRangeAsync(
                ticker,
                OhlcvConstants.Timeframes.M1,
                fromTime,
                newestTime.Value,
                cancellationToken);

            if (!sourceData.Any())
            {
                return new List<Domain.Entities.Ohlcv>();
            }

            var aggregated = OhlcvAggregationHelper.AggregateFromM1(sourceData, timeframe);
            return aggregated
                .OrderByDescending(candle => candle.Time)
                .Take(limit)
                .ToList();
        }

        private static string ToTimeframeString(VolumeTimeFrame timeframe)
        {
            return timeframe switch
            {
                VolumeTimeFrame.M1 => OhlcvConstants.Timeframes.M1,
                VolumeTimeFrame.M5 => OhlcvConstants.Timeframes.M5,
                VolumeTimeFrame.M15 => OhlcvConstants.Timeframes.M15,
                VolumeTimeFrame.M30 => OhlcvConstants.Timeframes.M30,
                VolumeTimeFrame.H1 => OhlcvConstants.Timeframes.H1,
                VolumeTimeFrame.H4 => OhlcvConstants.Timeframes.H4,
                VolumeTimeFrame.D1 => OhlcvConstants.Timeframes.D1,
                _ => OhlcvConstants.Timeframes.M1
            };
        }

        private sealed record VolumeSnapshot(decimal CurrentVolume, decimal? BaselineMinVolume);

        private async Task<int> GetOrCreateSystemSessionIdAsync(
            Guid userId,
            DateTimeOffset now,
            CancellationToken cancellationToken)
        {
            var systemSession = await _uow.ChatSessions.GetSystemSessionByUserIdAsync(userId, cancellationToken);

            if (systemSession == null)
            {
                systemSession = new ChatSession
                {
                    Title = ChatConstants.SystemNotificationSessionTitle,
                    SessionType = ChatSessionType.System,
                    Status = CommonStatus.Active,
                    CreatedBy = null,
                    CreatedAt = now,
                    UpdatedAt = now,
                };

                await _uow.ChatSessions.AddAsync(systemSession, cancellationToken);
                await _uow.SaveChangesAsync(cancellationToken);

                await _uow.ChatParticipants.AddAsync(new ChatParticipant
                {
                    SessionId = systemSession.Id,
                    UserId = userId,
                    Role = ChatRole.Member,
                    JoinedAt = now,
                    LastReadAt = now,
                }, cancellationToken);

                return systemSession.Id;
            }

            var isParticipant = await _uow.ChatParticipants.IsUserParticipantAsync(systemSession.Id, userId, cancellationToken);
            if (!isParticipant)
            {
                await _uow.ChatParticipants.AddAsync(new ChatParticipant
                {
                    SessionId = systemSession.Id,
                    UserId = userId,
                    Role = ChatRole.Member,
                    JoinedAt = now,
                    LastReadAt = now,
                }, cancellationToken);
            }

            systemSession.UpdatedAt = now;
            _uow.ChatSessions.Update(systemSession);

            return systemSession.Id;
        }

        private async Task<ChatMessage> SaveAlertChatMessageAsync(
            int systemSessionId,
            string message,
            DateTimeOffset now,
            CancellationToken cancellationToken)
        {
            var chatMessage = new ChatMessage
            {
                SessionId = systemSessionId,
                SenderId = null,
                Content = message,
                MessageType = ChatMessageType.Alert,
                CreatedAt = now,
                UpdatedAt = now,
            };

            await _uow.ChatMessages.AddAsync(chatMessage, cancellationToken);
            return chatMessage;
        }

        private static bool IsTriggered(
            Alert alert,
            decimal currentPrice,
            decimal currentVolume,
            decimal pricePercentUp,
            decimal pricePercentDown,
            decimal volumePercentUp,
            decimal volumePercentDown)
        {
            var monitoredValue = alert.Type == AlertType.Price ? currentPrice : currentVolume;
            var percentUpValue = alert.Type == AlertType.Price ? pricePercentUp : volumePercentUp;
            var percentDownValue = alert.Type == AlertType.Price ? pricePercentDown : volumePercentDown;

            if (alert.Condition is ConditionType.Above or ConditionType.Below && !alert.ThresholdValue.HasValue)
            {
                return false;
            }

            if (alert.Type == AlertType.Volume && alert.Condition is ConditionType.PercentChangeUp or ConditionType.PercentChangeDown)
            {
                if (!alert.ChangePercentage.HasValue)
                {
                    return false;
                }

                return alert.Condition == ConditionType.PercentChangeUp
                    ? percentUpValue >= alert.ChangePercentage.Value
                    : percentDownValue >= alert.ChangePercentage.Value;
            }

            if (alert.Type == AlertType.Price && alert.Condition is ConditionType.PercentChangeUp or ConditionType.PercentChangeDown)
            {
                if (!alert.ChangePercentage.HasValue)
                {
                    return false;
                }

                return alert.Condition == ConditionType.PercentChangeUp
                    ? percentUpValue >= alert.ChangePercentage.Value
                    : percentDownValue >= alert.ChangePercentage.Value;
            }

            return alert.Condition switch
            {
                ConditionType.Above => monitoredValue >= alert.ThresholdValue,
                ConditionType.Below => monitoredValue <= alert.ThresholdValue,
                ConditionType.PercentChangeUp => percentUpValue >= alert.ChangePercentage,
                ConditionType.PercentChangeDown => percentDownValue >= alert.ChangePercentage,
                _ => false,
            };
        }

        private static int? ParseAlertId(string value)
        {
            return int.TryParse(value, out var id) ? id : null;
        }

    }
}
