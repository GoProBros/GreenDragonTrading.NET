namespace GreenDragonTrading.Application.DTOs;

public class TelegramStartLinkDto
{
    public string StartToken { get; set; } = string.Empty;

    public string? DeepLink { get; set; }

    public string? BotUsername { get; set; }

    public int ExpiresInMinutes { get; set; }
}