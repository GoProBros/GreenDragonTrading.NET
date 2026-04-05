using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Telegram.Commands.ProcessTelegramWebhookUpdate;

public class ProcessTelegramWebhookUpdateCommandHandler(
    ITelegramLinkService telegramLinkService,
    ITelegramBotService telegramBotService,
    ILogger<ProcessTelegramWebhookUpdateCommandHandler> logger)
    : IRequestHandler<ProcessTelegramWebhookUpdateCommand, ApiResponse>
{
    private readonly ITelegramLinkService _telegramLinkService = telegramLinkService;
    private readonly ITelegramBotService _telegramBotService = telegramBotService;
    private readonly ILogger<ProcessTelegramWebhookUpdateCommandHandler> _logger = logger;

    public async Task<ApiResponse> Handle(
        ProcessTelegramWebhookUpdateCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ChatId) || string.IsNullOrWhiteSpace(request.MessageText))
        {
            return ApiResponse.Success();
        }

        if (!request.MessageText.StartsWith(TelegramConstants.StartCommand, StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse.Success();
        }

        var startToken = ExtractStartToken(request.MessageText);
        if (string.IsNullOrWhiteSpace(startToken))
        {
            await _telegramBotService.SendTextMessageAsync(
                request.ChatId,
                TelegramConstants.MissingStartTokenMessage,
                cancellationToken);

            return ApiResponse.Success();
        }

        var linkResult = await _telegramLinkService.LinkByStartTokenAsync(
            startToken,
            request.ChatId,
            cancellationToken);

        var replyMessage = linkResult.IsSuccess
            ? TelegramConstants.LinkSuccessBotReplyMessage
            : linkResult.Message;

        await _telegramBotService.SendTextMessageAsync(
            request.ChatId,
            replyMessage,
            cancellationToken);

        _logger.LogInformation(
            "Telegram start command processed. Success: {IsSuccess}, ChatId: {ChatId}, UserId: {UserId}",
            linkResult.IsSuccess,
            request.ChatId,
            linkResult.UserId);

        return ApiResponse.Success();
    }

    private static string ExtractStartToken(string messageText)
    {
        var parts = messageText.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 1 ? parts[1].Trim() : string.Empty;
    }
}