namespace GreenDragonTrading.Application.Interfaces;

public interface ITelegramBotService
{
    bool IsConfigured { get; }

    Task<bool> SendTextMessageAsync(
        string chatId,
        string message,
        CancellationToken cancellationToken = default);
}