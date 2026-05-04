using GreenDragonTrading.Application.DTOs;

namespace GreenDragonTrading.Application.Interfaces
{
    /// <summary>
    /// Service interface for communicating with the GreenDragonTrading AI Engine
    /// </summary>
    public interface IAiChatService
    {
        /// <summary>
        /// Submit a message to the AI engine and receive either immediate result or async job details.
        /// </summary>
        Task<AiChatSubmissionResult> SubmitMessageAsync(
            string conversationId,
            string message,
            AiChatContext? context = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Poll async job status from the AI engine.
        /// </summary>
        Task<AiChatJobStatusResponse?> GetJobStatusAsync(
            string pollUrl,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Send a message to the AI engine and receive a response
        /// </summary>
        Task<AiChatResponse> SendMessageAsync(
            string conversationId,
            string message,
            AiChatContext? context = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Compress conversation history into a bullet-point summary
        /// </summary>
        Task<AiConvSummaryResponse> UpdateSummaryAsync(
            string conversationId,
            string? existingSummary,
            List<AiMessageInput> newMessages,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Summarize an article content and extract related ticker entities.
        /// </summary>
        Task<AiNewsSummarizationResponse?> SummarizeNewsAsync(
            string title,
            string content,
            CancellationToken cancellationToken = default);
    }
}
