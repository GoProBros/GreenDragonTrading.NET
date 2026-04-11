using GreenDragonTrading.Application.DTOs.Realtime;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Infrastructure.Services
{
    /// <inheritdoc/>
    public class NotificationBroadcaster(
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationBroadcaster> logger) : INotificationBroadcaster
    {
        private readonly IHubContext<NotificationHub> _hubContext = hubContext;
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
        }
    }
}