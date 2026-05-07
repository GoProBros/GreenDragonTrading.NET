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
}
