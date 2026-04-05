using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using MediatR;
using Microsoft.Extensions.Options;

namespace GreenDragonTrading.Application.UseCases.Telegram.Queries.GenerateTelegramStartToken;

public class GenerateTelegramStartTokenQueryHandler(
    ICurrentUserService currentUserService,
    ITelegramLinkService telegramLinkService,
    IOptions<TelegramBotOptions> telegramOptions)
    : IRequestHandler<GenerateTelegramStartTokenQuery, ApiResponse<TelegramStartLinkDto>>
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly ITelegramLinkService _telegramLinkService = telegramLinkService;
    private readonly TelegramBotOptions _telegramOptions = telegramOptions.Value;

    public async Task<ApiResponse<TelegramStartLinkDto>> Handle(
        GenerateTelegramStartTokenQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();
        var startToken = await _telegramLinkService.CreateStartTokenAsync(userId, cancellationToken);

        var botUsername = NormalizeBotUsername(_telegramOptions.BotUsername);
        var response = new TelegramStartLinkDto
        {
            StartToken = startToken,
            BotUsername = botUsername,
            ExpiresInMinutes = Math.Max(1, _telegramOptions.StartTokenTtlMinutes),
            DeepLink = string.IsNullOrWhiteSpace(botUsername)
                ? null
                : $"{TelegramConstants.DeepLinkBaseUrl}/{botUsername}?start={startToken}"
        };

        return ApiResponse<TelegramStartLinkDto>.Success(
            response,
            TelegramConstants.StartTokenCreatedMessage);
    }

    private static string? NormalizeBotUsername(string? botUsername)
    {
        if (string.IsNullOrWhiteSpace(botUsername))
        {
            return null;
        }

        return botUsername.Trim().TrimStart('@');
    }
}