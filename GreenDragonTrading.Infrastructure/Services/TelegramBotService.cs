using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace GreenDragonTrading.Infrastructure.Services;

public class TelegramBotService : ITelegramBotService
{
    private readonly ILogger<TelegramBotService> _logger;
    private readonly ITelegramBotClient? _botClient;

    public TelegramBotService(
        IOptions<TelegramBotOptions> telegramOptions,
        ILogger<TelegramBotService> logger)
    {
        _logger = logger;
        var token = telegramOptions.Value.BotToken?.Trim();

        if (!string.IsNullOrWhiteSpace(token))
        {
            _botClient = new TelegramBotClient(token);
            IsConfigured = true;
            return;
        }

        _logger.LogWarning("Telegram bot token is missing. Telegram notifications are disabled.");
    }

    public bool IsConfigured { get; }

    public async Task<bool> SendTextMessageAsync(
        string chatId,
        string message,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured || _botClient == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(chatId) || string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        try
        {
            await _botClient.SendTextMessageAsync(
                chatId: chatId.Trim(),
                text: message,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Telegram message to chatId {ChatId}", chatId);
            return false;
        }
    }
}