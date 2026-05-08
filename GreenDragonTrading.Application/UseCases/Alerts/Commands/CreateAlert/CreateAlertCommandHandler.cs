using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Application.Common.Utils;
using GreenDragonTrading.Application.DTOs.Realtime;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using GreenDragonTrading.Application.UseCases.Alerts.Events;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Commands.CreateAlert
{
    public class CreateAlertCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        IRedisService redisService,
        INotificationBroadcaster notificationBroadcaster,
        ITelegramBotService telegramBotService,
        IMediator? mediator = null) : IRequestHandler<CreateAlertCommand, ApiResponse<AlertDto>>
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IRedisService _redisService = redisService;
        private readonly INotificationBroadcaster _notificationBroadcaster = notificationBroadcaster;
        private readonly ITelegramBotService _telegramBotService = telegramBotService;
        private readonly IMediator? _mediator = mediator;

        public async Task<ApiResponse<AlertDto>> Handle(CreateAlertCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetRequiredUserId();
            var ticker = request.Ticker.Trim().ToUpperInvariant();

            var symbol = await _uow.Symbols.GetByIdAsync(ticker, cancellationToken);
            if (symbol == null)
            {
                throw new NotFoundException("Mã cổ phiếu không tồn tại");
            }

            var isPercentCondition = request.Condition is ConditionType.PercentChangeUp or ConditionType.PercentChangeDown;
            MarketSymbolDto? currentMarketData = null;
            decimal? currentMonitoredValue = null;

            if (request.Type == AlertType.Price)
            {
                currentMarketData = await GetCurrentMarketDataAsync(request.Type, ticker);
                currentMonitoredValue = GetCurrentMonitoredValue(request.Type, currentMarketData);
            }

            var calculatedThreshold = CalculateThresholdValue(request, currentMonitoredValue);

            var alert = new Alert
            {
                UserId = userId,
                Ticker = ticker,
                Type = request.Type,
                Condition = request.Condition,
                ChangePercentage = isPercentCondition ? request.ChangePercentage : null,
                CurrentPrice = request.Type == AlertType.Price ? currentMonitoredValue : null,
                ThresholdValue = calculatedThreshold,
                VolumeTimeFrame = request.VolumeTimeFrame,
                VolumeLookbackBars = isPercentCondition && request.Type == AlertType.Volume
                    ? request.VolumeLookbackBars
                    : null,
                Name = request.Name?.Trim(),
                IsActive = request.IsActive,
                IsTriggered = false,
                ChatSessionId = request.ChatSessionId,
                MessageTemplate = string.IsNullOrWhiteSpace(request.MessageTemplate)
                    ? null
                    : request.MessageTemplate.Trim(),
                NotifyVia = NotificationChannel.System,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            };

            await _uow.Alerts.AddAsync(alert, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            if (alert.Type == AlertType.Price && alert.IsActive && !alert.IsTriggered && currentMarketData != null && currentMonitoredValue.HasValue)
            {
                // Calculate current values and percent changes (same logic as PriceUpdatedEventHandler)
                var currentPrice = currentMarketData.LastPrice > 0
                    ? (decimal)currentMarketData.LastPrice
                    : currentMonitoredValue.Value;

                var currentVolume = currentMarketData.TotalVol > 0
                    ? (decimal)currentMarketData.TotalVol
                    : 0m;

                var referencePrice = currentMarketData.ReferencePrice > 0
                    ? (decimal?)currentMarketData.ReferencePrice
                    : null;

                var previousVolume = currentMarketData.LastVol > 0
                    ? (decimal?)currentMarketData.LastVol
                    : null;

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

                // Evaluate trigger synchronously. If triggered, send notifications now and do NOT add to Redis.
                if (IsTriggered(
                    alert,
                    currentPrice,
                    currentVolume,
                    pricePercentUp,
                    pricePercentDown,
                    volumePercentUp,
                    volumePercentDown))
                {
                    var now = DateTimeOffset.UtcNow;
                    alert.IsTriggered = true;
                    alert.LastTriggeredAt = now;
                    alert.UpdatedAt = now;
                    _uow.Alerts.Update(alert);

                    // For Price Above/Below, compute percent relative to threshold
                    var effectivePricePercentUp = pricePercentUp;
                    var effectivePricePercentDown = pricePercentDown;
                    if (alert.ThresholdValue.HasValue && alert.ThresholdValue.Value > 0
                        && alert.Condition is ConditionType.Above or ConditionType.Below)
                    {
                        if (alert.Condition == ConditionType.Above)
                        {
                            effectivePricePercentUp = ((currentPrice - alert.ThresholdValue.Value) / alert.ThresholdValue.Value) * 100m;
                            effectivePricePercentDown = 0m;
                        }
                        else
                        {
                            effectivePricePercentDown = ((alert.ThresholdValue.Value - currentPrice) / alert.ThresholdValue.Value) * 100m;
                            effectivePricePercentUp = 0m;
                        }
                    }

                    // Resolve template and build message
                    var template = await ResolveTemplateAsync(alert.Type, alert.Condition, cancellationToken);
                    var message = AlertTemplateRenderingHelper.BuildAlertMessage(
                        alert,
                        currentPrice,
                        currentVolume,
                        effectivePricePercentUp,
                        effectivePricePercentDown,
                        volumePercentUp,
                        volumePercentDown,
                        template);

                    // Ensure system session exists and save message
                    var systemSessionId = await GetOrCreateSystemSessionIdAsync(alert.UserId, now, cancellationToken);
                    var savedMessage = await SaveAlertChatMessageAsync(systemSessionId, message, now, cancellationToken);

                    await _uow.SaveChangesAsync(cancellationToken);

                    // Broadcast and Telegram
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
                            Ticker = alert.Ticker,
                        },
                        cancellationToken);

                    var telegramChatIdsByUser = await GetTelegramChatIdsAsync(new List<Alert> { alert }, cancellationToken);
                    await TrySendTelegramAsync(telegramChatIdsByUser, alert.UserId, message, cancellationToken);
                }
                else
                {
                    // Not triggered now -> add to Redis for background processing
                    await _redisService.SortedSetAddAsync(
                        GetRedisKey(alert),
                        alert.Id.ToString(),
                        (double)(alert.ThresholdValue ?? 0m));
                }
            }
            else if (alert.IsActive && !alert.IsTriggered)
            {
                await _redisService.SortedSetAddAsync(
                    GetRedisKey(alert),
                    alert.Id.ToString(),
                    (double)(alert.ThresholdValue ?? 0m));
            }

            var dto = MapToDto(alert);
            return ApiResponse<AlertDto>.Success(dto, "Đặt cảnh báo thành công");
        }

        private static string GetRedisKey(Alert alert)
        {
            return RedisConstants.AlertsByTypeAndCondition(alert.Ticker, alert.Type, alert.Condition);
        }

        private async Task<MarketSymbolDto> GetCurrentMarketDataAsync(AlertType type, string ticker)
        {
            var marketData = await _redisService.GetHashAsync<MarketSymbolDto>(
                RedisConstants.MarketDataSymbol(ticker));

            if (marketData == null)
            {
                throw new BusinessRuleException("Không tìm thấy dữ liệu thị trường hiện tại trên Redis");
            }

            return marketData;
        }

        private static decimal GetCurrentMonitoredValue(AlertType type, MarketSymbolDto marketData)
        {
            var monitoredValue = type == AlertType.Price
                ? (decimal)marketData.LastPrice
                : (decimal)marketData.TotalVol;

            if (monitoredValue <= 0)
            {
                throw new BusinessRuleException(
                    type == AlertType.Price
                        ? "Giá hiện tại chưa sẵn sàng để đặt cảnh báo"
                        : "Khối lượng hiện tại chưa sẵn sàng để đặt cảnh báo");
            }

            return monitoredValue;
        }

        private static decimal? CalculateThresholdValue(CreateAlertCommand request, decimal? currentMonitoredValue)
        {
            if (request.Condition is ConditionType.Above or ConditionType.Below)
            {
                if (!request.ThresholdValue.HasValue)
                {
                    throw new BusinessRuleException("Condition 1,2 yêu cầu thresholdValue");
                }

                return request.ThresholdValue.Value;
            }

            if (request.Type == AlertType.Price)
            {
                if (!request.ChangePercentage.HasValue)
                {
                    throw new BusinessRuleException("Condition 3,4 yêu cầu changePercentage");
                }

                if (!currentMonitoredValue.HasValue)
                {
                    throw new BusinessRuleException("Giá hiện tại chưa sẵn sàng để đặt cảnh báo");
                }

                var pct = request.ChangePercentage.Value;
                var factor = pct / 100m;

                return request.Condition switch
                {
                    ConditionType.PercentChangeUp => currentMonitoredValue.Value * (1m + factor),
                    ConditionType.PercentChangeDown => currentMonitoredValue.Value * (1m - factor),
                    _ => throw new BusinessRuleException("Condition không hợp lệ"),
                };
            }

            return null;
        }

        private async Task<AlertTemplate?> ResolveTemplateAsync(AlertType type, ConditionType condition, CancellationToken cancellationToken)
        {
            var cached = await _uow.AlertTemplates.GetActiveByTypeAndConditionAsync(type, condition, cancellationToken);
            if (cached != null) return cached;

            return await _uow.AlertTemplates.GetDefaultAsync(cancellationToken);
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
                    Status = Domain.Enums.CommonStatus.Active,
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

        private static AlertDto MapToDto(Alert alert)
        {
            return new AlertDto
            {
                Id = alert.Id,
                UserId = alert.UserId,
                Ticker = alert.Ticker,
                Type = alert.Type,
                Condition = alert.Condition,
                ChangePercentage = alert.ChangePercentage,
                ThresholdValue = alert.ThresholdValue,
                VolumeTimeFrame = alert.VolumeTimeFrame,
                VolumeLookbackBars = alert.VolumeLookbackBars,
                Name = alert.Name,
                IsActive = alert.IsActive,
                IsTriggered = alert.IsTriggered,
                LastTriggeredAt = alert.LastTriggeredAt,
                ChatSessionId = alert.ChatSessionId,
                MessageTemplate = alert.MessageTemplate,
                CreatedAt = alert.CreatedAt,
                UpdatedAt = alert.UpdatedAt,
            };
        }
    }
}
