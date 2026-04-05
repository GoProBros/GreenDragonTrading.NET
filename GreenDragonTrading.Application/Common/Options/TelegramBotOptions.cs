namespace GreenDragonTrading.Application.Common.Options;

public class TelegramBotOptions
{
    public const string SectionName = "TelegramBot";

    public string BotToken { get; set; } = string.Empty;

    public string? BotUsername { get; set; }

    public string? WebhookSecretToken { get; set; }

    public int StartTokenTtlMinutes { get; set; } = 10;
}