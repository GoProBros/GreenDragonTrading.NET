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
        public DateTimeOffset? LastReadAt { get; set; }
        public int? LastReadMessageId { get; set; }
        public List<ChatMessageDto> Messages { get; set; } = new();
    }

    public class ChatMessageDto
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public Guid? SenderId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? ResponseData { get; set; }
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

    public class SendChatMessageResultDto
    {
        public bool Accepted { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? JobId { get; set; }
        public string? PollUrl { get; set; }
        public string? IntentHint { get; set; }
        public ChatMessageDto UserMessage { get; set; } = default!;
        public SendChatMessageResponseDto? Result { get; set; }
    }

    public class ChatAsyncJobStatusDto
    {
        public bool Success { get; set; }
        public bool Accepted { get; set; }
        public string JobId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public SendChatMessageResponseDto? Result { get; set; }
        public string? Error { get; set; }
    }

    public class AiIntentDto
    {
        public string Intent { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }

    public class ChatMessageSimpleDto
    {
        public int Id { get; set; }
        public string Role { get; set; } = string.Empty;
        public Guid? SenderId { get; set; }
        public string? SenderName { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? ResponseData { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public bool IsUnreadForCurrentUser { get; set; }
    }

    public class ChatSessionDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public ChatSessionType SessionType { get; set; }
        public string? Summary { get; set; }
        public DateTimeOffset? LastReadAt { get; set; }
        public int? LastReadMessageId { get; set; }
        public DirectChatParticipantDto? OtherParticipant { get; set; }
        public List<ChatMessageSimpleDto> Messages { get; set; } = new();
    }

    public class GetOrCreateDirectSessionRequestDto
    {
        public string PhoneOrEmail { get; set; } = string.Empty;
    }

    public class DirectChatParticipantDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }

    public class DirectChatSessionDto
    {
        public int SessionId { get; set; }
        public bool IsNew { get; set; }
        public DirectChatParticipantDto OtherParticipant { get; set; } = default!;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? MyLastReadAt { get; set; }
        public int? MyLastReadMessageId { get; set; }
    }

    public class DirectSessionListItemDto
    {
        public int SessionId { get; set; }
        public DirectChatParticipantDto OtherParticipant { get; set; } = default!;
        public string? LastMessageContent { get; set; }
        public Guid? LastMessageSenderId { get; set; }
        public DateTimeOffset? LastMessageAt { get; set; }
        public DateTimeOffset? MyLastReadAt { get; set; }
        public int? MyLastReadMessageId { get; set; }
        public bool HasUnread { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public class SendDirectMessageRequestDto
    {
        public string Content { get; set; } = string.Empty;
    }

    public class DirectMessageDto
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public bool IsFromCurrentUser { get; set; }
    }

    public class SummarizeSessionResponseDto
    {
        public int SessionId { get; set; }
        public string UpdatedSummary { get; set; } = string.Empty;
        public int LastSummaryMessageId { get; set; }
        public int ProcessedMessages { get; set; }
    }

    public class SendSystemNotificationRequestDto
    {
        public List<Guid>? UserIds { get; set; }
        public bool SendToAll { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class SendSystemNotificationResponseDto
    {
        public int? SessionId { get; set; }
        public int? MessageId { get; set; }
        public Guid? UserId { get; set; }
        public List<Guid>? UserIds { get; set; }
        public bool SentToAll { get; set; }
        public int SentCount { get; set; }
        public string Message { get; set; } = string.Empty;
        public ChatMessageType MessageType { get; set; }
        public ChatSessionType SessionType { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
