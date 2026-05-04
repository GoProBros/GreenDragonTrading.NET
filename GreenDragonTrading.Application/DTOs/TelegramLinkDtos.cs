namespace GreenDragonTrading.Application.DTOs
{
    public sealed class TelegramLinkResult
    {
        public bool IsSuccess { get; init; }
        public string Message { get; init; } = string.Empty;
        public Guid? UserId { get; init; }
    }
}
