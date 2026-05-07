using System.Text.Json.Serialization;

namespace GreenDragonTrading.Application.DTOs
{
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

    /// <summary>
    /// Response from AI engine suggestions endpoint.
    /// </summary>
    public class AiChatSuggestionsResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("questions")]
        public List<string> Questions { get; set; } = new();

        [JsonPropertyName("sourceSymbols")]
        public List<string> SourceSymbols { get; set; } = new();

        [JsonPropertyName("metadata")]
        public AiChatSuggestionsMetadata? Metadata { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }

    /// <summary>
    /// Metadata for AI suggestions response.
    /// </summary>
    public class AiChatSuggestionsMetadata
    {
        [JsonPropertyName("modelName")]
        public string ModelName { get; set; } = string.Empty;

        [JsonPropertyName("latencyMs")]
        public long LatencyMs { get; set; }
    }
}
