using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Application.DTOs
{
    public class ChatSessionDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public ChatSessionType SessionType { get; set; }
        public CommonStatus Status { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public List<ChatMessageDto> Messages { get; set; } = new();
    }

    public class ChatMessageDto
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public Guid? SenderId { get; set; }
        public string Content { get; set; } = string.Empty;
        public ChatMessageType MessageType { get; set; }
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public class CreateChatSessionRequestDto
    {
        public string? Title { get; set; }
    }

    public class ChatSessionListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public ChatSessionType SessionType { get; set; }
        public int ParticipantCount { get; set; }
        public string? CreatorName { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public class SendChatMessageRequestDto
    {
        public string Message { get; set; } = string.Empty;
    }

    public class SendChatMessageResponseDto
    {
        public ChatMessageDto UserMessage { get; set; } = default!;
        public ChatMessageDto AiMessage { get; set; } = default!;
        public List<AiIntentDto>? Intents { get; set; }
    }

    public class AiIntentDto
    {
        public string Intent { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }

    public class ChatMessageSimpleDto
    {
        /// <summary>"user" if sent by a user, "ai" if sent by the AI (SenderId is null)</summary>
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class ChatSessionDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public List<ChatMessageSimpleDto> Messages { get; set; } = new();
    }

    /// <summary>
    public class SummarizeSessionResponseDto
    {
        public int SessionId { get; set; }
        public string UpdatedSummary { get; set; } = string.Empty;
        public int LastSummaryMessageId { get; set; }
        public int ProcessedMessages { get; set; }
    }
}
