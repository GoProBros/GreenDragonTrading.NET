using GreenDragonTrading.Application.DTOs;

namespace GreenDragonTrading.Application.Interfaces
{
    public interface IChatAsyncJobService
    {
        Task EnqueueAsync(ChatAsyncProcessingJob job, CancellationToken cancellationToken = default);
        ValueTask<ChatAsyncProcessingJob> DequeueAsync(CancellationToken cancellationToken = default);
        Task<ChatAsyncJobState?> GetStatusAsync(string jobId, CancellationToken cancellationToken = default);
        Task MarkRunningAsync(string jobId, CancellationToken cancellationToken = default);
        Task MarkCompletedAsync(string jobId, SendChatMessageResponseDto result, CancellationToken cancellationToken = default);
        Task MarkFailedAsync(string jobId, string error, CancellationToken cancellationToken = default);
        Task CleanupExpiredAsync(CancellationToken cancellationToken = default);
    }

    public class ChatAsyncProcessingJob
    {
        public string JobId { get; set; } = string.Empty;
        public string PollUrl { get; set; } = string.Empty;
        public string ConversationId { get; set; } = string.Empty;
        public int SessionId { get; set; }
        public Guid UserId { get; set; }
        public int UserMessageId { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }

    public class ChatAsyncJobState
    {
        public string JobId { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public int SessionId { get; set; }
        public int UserMessageId { get; set; }
        public string Status { get; set; } = ChatAsyncJobStatuses.Queued;
        public bool Success { get; set; } = true;
        public bool Accepted { get; set; } = true;
        public string? Error { get; set; }
        public SendChatMessageResponseDto? Result { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddMinutes(30);
    }

    public static class ChatAsyncJobStatuses
    {
        public const string Queued = "queued";
        public const string Running = "running";
        public const string Completed = "completed";
        public const string Failed = "failed";
    }
}
