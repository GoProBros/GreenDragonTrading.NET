using GreenDragonTrading.Application.DTOs.Realtime;

namespace GreenDragonTrading.Application.Interfaces
{
    /// <summary>
    /// Broadcasts notification and message events to clients through SignalR.
    /// </summary>
    public interface INotificationBroadcaster
    {
        /// <summary>
        /// Sends a system chat message event to a specific user.
        /// Event name: ReceiveSystemChatMessage
        /// </summary>
        Task BroadcastSystemChatMessageAsync(
            Guid userId,
            SystemChatMessageSignalREventDto payload,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an AI chat response event to a specific user.
        /// Event name: ReceiveAiChatResponse
        /// </summary>
        Task BroadcastAiChatResponseAsync(
            Guid userId,
            AiChatResponseSignalREventDto payload,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a direct message event to a specific user.
        /// Event name: ReceiveDirectMessage
        /// </summary>
        Task BroadcastDirectMessageAsync(
            Guid userId,
            DirectMessageSignalREventDto payload,
            CancellationToken cancellationToken = default);
    }
}