namespace GreenDragonTrading.Application.DTOs.Realtime
{
    /// <summary>
    /// Event payload sent when a new message is created in a user's System chat session.
    /// </summary>
    public class SystemChatMessageSignalREventDto
    {
        public int SessionId { get; set; }
        public int MessageId { get; set; }
        public string MessageType { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public int? AlertId { get; set; }
        public string? Ticker { get; set; }
    }

    /// <summary>
    /// Event payload sent when AI generated a response in an AI chat session.
    /// </summary>
    public class AiChatResponseSignalREventDto
    {
        public int SessionId { get; set; }
        public int UserMessageId { get; set; }
        public int AiMessageId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }

    /// <summary>
    /// Event payload sent when a direct message is created in a direct chat session.
    /// </summary>
    public class DirectMessageSignalREventDto
    {
        public int SessionId { get; set; }
        public int MessageId { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}