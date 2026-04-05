namespace GreenDragonTrading.Application.Interfaces;

public interface ITelegramLinkService
{
    Task<string> CreateStartTokenAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<TelegramLinkResult> LinkByStartTokenAsync(
        string startToken,
        string chatId,
        CancellationToken cancellationToken = default);
}

public sealed class TelegramLinkResult
{
    public bool IsSuccess { get; init; }

    public string Message { get; init; } = string.Empty;

    public Guid? UserId { get; init; }
}