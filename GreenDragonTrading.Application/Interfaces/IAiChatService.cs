using System.Text.Json.Serialization;

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

    /// <summary>
    /// Request payload for AI engine chat endpoint
    /// </summary>
    public class AiChatRequest
    {
        public string ConversationId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public AiChatContext? Context { get; set; }
        
        [JsonPropertyName("executionMode")]
        public string ExecutionMode { get; set; } = "auto";
    }

    /// <summary>
    /// Response when chat job is accepted asynchronously
    /// </summary>
    public class AiChatAcceptedResponse
    {
        public bool Success { get; set; }
        public bool Accepted { get; set; }
        public string JobId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ConversationId { get; set; } = string.Empty;
        public string? IntentHint { get; set; }
        public string PollUrl { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result of submitting a chat request to AI engine.
    /// </summary>
    public class AiChatSubmissionResult
    {
        public bool Success { get; set; }
        public bool Accepted { get; set; }
        public AiChatAcceptedResponse? AcceptedResponse { get; set; }
        public AiChatResponse? Response { get; set; }
        public string? Error { get; set; }
    }

    /// <summary>
    /// Status response when polling an async chat job
    /// </summary>
    public class AiChatJobStatusResponse
    {
        public bool Success { get; set; }
        public bool Accepted { get; set; }
        public string Status { get; set; } = string.Empty;
        public AiChatResponse? Result { get; set; }
        public string? Error { get; set; }
    }

    /// <summary>
    /// Context passed along with each chat message (recent history + summary)
    /// </summary>
    public class AiChatContext
    {
        public string? Summary { get; set; }
        public List<AiMessageInput> RecentMessages { get; set; } = new();
    }

    /// <summary>
    /// A single message entry for context or summary payloads
    /// </summary>
    public class AiMessageInput
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response from AI engine chat endpoint
    /// </summary>
    public class AiChatResponse
    {
        public bool Success { get; set; }
        public string? ConversationId { get; set; }
        public List<AiIntentResult>? Intents { get; set; }
        public string? Response { get; set; }
        public object? ResponseData { get; set; }
        public string? Error { get; set; }
    }

    /// <summary>
    /// Intent classification result from AI engine
    /// </summary>
    public class AiIntentResult
    {
        public string Intent { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }

    /// <summary>
    /// Request payload for conv-summary endpoint
    /// </summary>
    public class AiConvSummaryRequest
    {
        public string ConversationId { get; set; } = string.Empty;
        public string? ExistingSummary { get; set; }
        public List<AiMessageInput> NewMessages { get; set; } = new();
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Response from conv-summary endpoint
    /// </summary>
    public class AiConvSummaryResponse
    {
        public bool Success { get; set; }
        public string? ConversationId { get; set; }
        public string? UpdatedSummary { get; set; }
        public int ProcessedMessages { get; set; }
        public string? Error { get; set; }
    }

    /// <summary>
    /// Request payload for news summarization endpoint.
    /// </summary>
    public class AiNewsSummarizationRequest
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response payload from news summarization endpoint.
    /// </summary>
    public class AiNewsSummarizationResponse
    {
        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        [JsonPropertyName("entities")]
        public List<AiNewsEntity> Entities { get; set; } = new();
    }

    public class AiNewsEntity
    {
        [JsonPropertyName("ticker")]
        public string? Ticker { get; set; }

        [JsonPropertyName("relevance_score")]
        public decimal? RelevanceScore { get; set; }

        [JsonPropertyName("sentiment_score")]
        public decimal? SentimentScore { get; set; }
    }
}
