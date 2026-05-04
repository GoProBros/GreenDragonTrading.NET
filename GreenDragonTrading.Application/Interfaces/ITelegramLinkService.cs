using GreenDragonTrading.Application.DTOs;

namespace GreenDragonTrading.Application.Interfaces;

public interface ITelegramLinkService
{
    Task<string> CreateStartTokenAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<TelegramLinkResult> LinkByStartTokenAsync(
        string startToken,
        string chatId,
        CancellationToken cancellationToken = default);
}