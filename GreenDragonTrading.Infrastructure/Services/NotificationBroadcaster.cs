using GreenDragonTrading.Application.DTOs.Realtime;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Interfaces;
using GreenDragonTrading.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Infrastructure.Services
{
    /// <inheritdoc/>
    public class NotificationBroadcaster(
        IHubContext<NotificationHub> hubContext,
        IExpoPushService expoPushService,
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationBroadcaster> logger) : INotificationBroadcaster
    {
        private readonly IHubContext<NotificationHub> _hubContext = hubContext;
        private readonly IExpoPushService _expoPushService = expoPushService;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<NotificationBroadcaster> _logger = logger;

        /// <inheritdoc/>
        public async Task BroadcastSystemChatMessageAsync(
            Guid userId,
            SystemChatMessageSignalREventDto payload,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _hubContext.Clients
                    .User(userId.ToString())
                    .SendAsync("ReceiveSystemChatMessage", payload, cancellationToken);

                _logger.LogDebug(
                    "Broadcasted system chat message event to user {UserId}. SessionId={SessionId}, MessageId={MessageId}",
                    userId,
                    payload.SessionId,
                    payload.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error broadcasting system chat message event to user {UserId}. SessionId={SessionId}, MessageId={MessageId}",
                    userId,
                    payload.SessionId,
                    payload.MessageId);
            }

            // FCM push: delivers to device even when app is backgrounded or killed.
            var title = payload.Source == "PriceAlert" || payload.MessageType == "Alert"
                ? $"C\u1ea3nh b\u00e1o gi\u00e1{(payload.Ticker != null ? $" \u2014 {payload.Ticker}" : "")}"
                : "Th\u00f4ng b\u00e1o h\u1ec7 th\u1ed1ng";
            _ = SendFcmAsync(userId, title, payload.Content, "kafistock://notifications");
        }

        /// <inheritdoc/>
        public async Task BroadcastAiChatResponseAsync(
            Guid userId,
            AiChatResponseSignalREventDto payload,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _hubContext.Clients
                    .User(userId.ToString())
                    .SendAsync("ReceiveAiChatResponse", payload, cancellationToken);

                _logger.LogDebug(
                    "Broadcasted AI chat response event to user {UserId}. SessionId={SessionId}, AiMessageId={AiMessageId}",
                    userId,
                    payload.SessionId,
                    payload.AiMessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error broadcasting AI chat response event to user {UserId}. SessionId={SessionId}, AiMessageId={AiMessageId}",
                    userId,
                    payload.SessionId,
                    payload.AiMessageId);
            }
            // AI responses are foreground-only \u2014 no FCM push.
        }

        /// <inheritdoc/>
        public async Task BroadcastDirectMessageAsync(
            Guid userId,
            DirectMessageSignalREventDto payload,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _hubContext.Clients
                    .User(userId.ToString())
                    .SendAsync("ReceiveDirectMessage", payload, cancellationToken);

                _logger.LogDebug(
                    "Broadcasted direct message event to user {UserId}. SessionId={SessionId}, MessageId={MessageId}",
                    userId,
                    payload.SessionId,
                    payload.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error broadcasting direct message event to user {UserId}. SessionId={SessionId}, MessageId={MessageId}",
                    userId,
                    payload.SessionId,
                    payload.MessageId);
            }

            // FCM push for direct messages.
            _ = SendFcmAsync(
                userId,
                title: payload.SenderName,
                body: payload.Content,
                dataUrl: $"kafistock://chat/{payload.SessionId}");
        }

        // \u2500\u2500\u2500 FCM Helpers \u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500

        /// <summary>
        /// Fire-and-forget FCM push. Uses a fresh DI scope for DB token look-up.
        /// Stale tokens reported by Expo (DeviceNotRegistered) are purged automatically.
        /// </summary>
        private async Task SendFcmAsync(Guid userId, string title, string body, string dataUrl)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var tokenEntities = await uow.UserPushTokens.GetByUserIdAsync(userId, CancellationToken.None);
                if (tokenEntities.Count == 0) return;

                var tokens = tokenEntities.Select(t => t.Token).ToList();
                var stale = await _expoPushService.SendAsync(tokens, title, body, dataUrl, CancellationToken.None);

                if (stale.Count > 0)
                {
                    foreach (var token in stale)
                        await uow.UserPushTokens.RemoveByTokenAsync(token, CancellationToken.None);
                    await uow.SaveChangesAsync(CancellationToken.None);

                    _logger.LogInformation(
                        "[NotificationBroadcaster] Purged {Count} stale FCM tokens for user {UserId}",
                        stale.Count, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[NotificationBroadcaster] Failed to send FCM push to user {UserId}", userId);
            }
        }
    }
}