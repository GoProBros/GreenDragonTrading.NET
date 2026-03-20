using GreenDragonTrading.Application.Interfaces;
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
        IUnitOfWork uow,
        IMarketDataBroadcaster marketDataBroadcaster) : INotificationHandler<PriceUpdatedEvent>
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> TickerLocks = new();

        private readonly IRedisService _redisService = redisService;
        private readonly IUnitOfWork _uow = uow;
        private readonly IMarketDataBroadcaster _marketDataBroadcaster = marketDataBroadcaster;

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
                alertIds.AddRange(await GetAlertIdsAsync(ticker, AlertType.Volume, ConditionType.Above, double.NegativeInfinity, (double)currentVolume));
                alertIds.AddRange(await GetAlertIdsAsync(ticker, AlertType.Volume, ConditionType.Below, (double)currentVolume, double.PositiveInfinity));

                alertIds.AddRange(await GetAlertIdsAsync(ticker, AlertType.Price, ConditionType.PercentChangeUp, double.NegativeInfinity, (double)currentPrice));
                alertIds.AddRange(await GetAlertIdsAsync(ticker, AlertType.Price, ConditionType.PercentChangeDown, (double)currentPrice, double.PositiveInfinity));

                alertIds.AddRange(await GetAlertIdsAsync(ticker, AlertType.Volume, ConditionType.PercentChangeUp, double.NegativeInfinity, (double)currentVolume));
                alertIds.AddRange(await GetAlertIdsAsync(ticker, AlertType.Volume, ConditionType.PercentChangeDown, (double)currentVolume, double.PositiveInfinity));

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

                var now = DateTimeOffset.UtcNow;
                var hasChanges = false;
                var systemSessionIdsByUser = new Dictionary<Guid, int>();

                foreach (var alert in alerts)
                {
                    if (!IsTriggered(
                        alert,
                        currentPrice,
                        currentVolume,
                        pricePercentUp,
                        pricePercentDown,
                        volumePercentUp,
                        volumePercentDown))
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

                    var message = BuildAlertMessage(
                        alert,
                        currentPrice,
                        currentVolume,
                        pricePercentUp,
                        pricePercentDown,
                        volumePercentUp,
                        volumePercentDown);

                    if (!systemSessionIdsByUser.TryGetValue(alert.UserId, out var systemSessionId))
                    {
                        systemSessionId = await GetOrCreateSystemSessionIdAsync(alert.UserId, now, cancellationToken);
                        systemSessionIdsByUser[alert.UserId] = systemSessionId;
                    }

                    var savedMessage = await SaveAlertChatMessageAsync(systemSessionId, message, now, cancellationToken);

                    await _marketDataBroadcaster.BroadcastChatMessageAsync(alert.UserId, new
                    {
                        MessageId = savedMessage.Id,
                        SessionId = systemSessionId,
                        SenderId = (Guid?)null,
                        Content = savedMessage.Content,
                        MessageType = savedMessage.MessageType.ToString(),
                        CreatedAt = savedMessage.CreatedAt,
                        UpdatedAt = savedMessage.UpdatedAt,
                        AlertId = alert.Id,
                        Ticker = ticker,
                        CurrentPrice = currentPrice,
                        CurrentVolume = currentVolume,
                        BasePrice = alert.CurrentPrice,
                        ChangePercentage = alert.ChangePercentage,
                        ThresholdValue = alert.ThresholdValue,
                        AlertType = alert.Type.ToString(),
                        Condition = alert.Condition.ToString(),
                        Message = message,
                        TriggeredAt = now,
                        ChatSessionId = systemSessionId
                    }, cancellationToken);
                }

                if (hasChanges)
                {
                    await _uow.SaveChangesAsync(cancellationToken);
                }
            }
            finally
            {
                tickerLock.Release();
            }
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
                    Title = "System Notification",
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

            if (alert.Type == AlertType.Volume && alert.Condition is ConditionType.PercentChangeUp or ConditionType.PercentChangeDown)
            {
                return alert.Condition == ConditionType.PercentChangeUp
                    ? currentVolume >= alert.ThresholdValue
                    : currentVolume <= alert.ThresholdValue;
            }

            if (alert.Type == AlertType.Price && alert.Condition is ConditionType.PercentChangeUp or ConditionType.PercentChangeDown)
            {
                return alert.Condition == ConditionType.PercentChangeUp
                    ? currentPrice >= alert.ThresholdValue
                    : currentPrice <= alert.ThresholdValue;
            }

            return alert.Condition switch
            {
                ConditionType.Above => monitoredValue >= alert.ThresholdValue,
                ConditionType.Below => monitoredValue <= alert.ThresholdValue,
                ConditionType.PercentChangeUp => percentUpValue >= alert.ThresholdValue,
                ConditionType.PercentChangeDown => percentDownValue >= alert.ThresholdValue,
                _ => false,
            };
        }

        private static int? ParseAlertId(string value)
        {
            return int.TryParse(value, out var id) ? id : null;
        }

        private static string BuildAlertMessage(
            Alert alert,
            decimal currentPrice,
            decimal currentVolume,
            decimal pricePercentUp,
            decimal pricePercentDown,
            decimal volumePercentUp,
            decimal volumePercentDown)
        {
            var price = currentPrice.ToString("0.####");
            var volume = currentVolume.ToString("0.####");
            var threshold = alert.ThresholdValue.ToString("0.####");
            decimal displayedPricePercentUp = pricePercentUp;
            decimal displayedPricePercentDown = pricePercentDown;
            decimal displayedVolumePercentUp = volumePercentUp;
            decimal displayedVolumePercentDown = volumePercentDown;

            if (alert.Type == AlertType.Price && alert.CurrentPrice > 0)
            {
                var changePercent = ((currentPrice - alert.CurrentPrice) / alert.CurrentPrice) * 100m;
                displayedPricePercentUp = Math.Max(changePercent, 0m);
                displayedPricePercentDown = Math.Max(-changePercent, 0m);
            }

            if (alert.Type == AlertType.Volume && alert.CurrentPrice > 0)
            {
                var changePercent = ((currentVolume - alert.CurrentPrice) / alert.CurrentPrice) * 100m;
                displayedVolumePercentUp = Math.Max(changePercent, 0m);
                displayedVolumePercentDown = Math.Max(-changePercent, 0m);
            }

            var percentUp = (alert.Type == AlertType.Price ? displayedPricePercentUp : displayedVolumePercentUp).ToString("0.##");
            var percentDown = (alert.Type == AlertType.Price ? displayedPricePercentDown : displayedVolumePercentDown).ToString("0.##");
            var configuredPercent = (alert.ChangePercentage ?? 0m).ToString("0.##");

            if (alert.Type == AlertType.Volume)
            {
                return alert.Condition switch
                {
                    ConditionType.Above => $"Cảnh báo khối lượng: {alert.Ticker} đạt {volume} và vượt ngưỡng {threshold}.",
                    ConditionType.Below => $"Cảnh báo khối lượng: {alert.Ticker} đạt {volume} và giảm dưới ngưỡng {threshold}.",
                    ConditionType.PercentChangeUp => $"Cảnh báo khối lượng: {alert.Ticker} tăng {percentUp}% và vượt mức {threshold}.",
                    ConditionType.PercentChangeDown => $"Cảnh báo khối lượng: {alert.Ticker} giảm {percentDown}% và vượt mức {threshold}.",
                    _ => $"Cảnh báo khối lượng: {alert.Ticker} đã đạt điều kiện.",
                };
            }

            return alert.Condition switch
            {
                ConditionType.Above => $"Cảnh báo: {alert.Ticker} đã tăng lên {price} và đạt ngưỡng {threshold}.",
                ConditionType.Below => $"Cảnh báo: {alert.Ticker} đã giảm xuống {price} và chạm ngưỡng {threshold}.",
                ConditionType.PercentChangeUp => $"Cảnh báo: {alert.Ticker} đã tăng {percentUp}% (mức đặt cảnh báo {configuredPercent}%), giá hiện tại {price}, giá mục tiêu {threshold}.",
                ConditionType.PercentChangeDown => $"Cảnh báo: {alert.Ticker} da giam {percentDown}% (mức đặt cảnh báo {configuredPercent}%), giá hiện tại {price}, giá mục tiêu {threshold}.",
                _ => $"Cảnh báo: {alert.Ticker} đặt điều kiện tại mức giá  {price}.",
            };
        }
    }
}
