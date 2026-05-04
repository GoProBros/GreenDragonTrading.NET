using System.Text.Json;
using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.Telegram.Commands.ProcessTelegramWebhookUpdate;
using GreenDragonTrading.Application.UseCases.Telegram.Queries.GenerateTelegramStartToken;
using GreenDragonTrading.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GreenDragonTrading.Api.Controllers;

[ApiController]
[Route("api/v1/telegram")]
public class TelegramController(
    IMediator mediator,
    IOptions<TelegramBotOptions> telegramOptions) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly TelegramBotOptions _telegramOptions = telegramOptions.Value;

    /// <summary>
    /// Generates a short-lived Telegram start token and deep-link for current authenticated user.
    /// </summary>
    [HttpGet("start-token")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<TelegramStartLinkDto>>> GenerateStartToken(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GenerateTelegramStartTokenQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Receives Telegram webhook updates and links chat id when user sends /start &lt;st&gt;.
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> ReceiveWebhook(
        [FromBody] JsonElement update,
        CancellationToken cancellationToken)
    {
        if (!IsWebhookSecretValid())
        {
            return Unauthorized();
        }

        var (chatId, messageText) = ExtractMessage(update);
        if (string.IsNullOrWhiteSpace(chatId) || string.IsNullOrWhiteSpace(messageText))
        {
            return Ok();
        }

        var result = await _mediator.Send(
            new ProcessTelegramWebhookUpdateCommand(chatId, messageText),
            cancellationToken);

        return Ok(result);
    }

    private bool IsWebhookSecretValid()
    {
        if (string.IsNullOrWhiteSpace(_telegramOptions.WebhookSecretToken))
        {
            return true;
        }

        var requestToken = Request.Headers[TelegramConstants.WebhookSecretHeaderName].ToString();
        return string.Equals(
            requestToken,
            _telegramOptions.WebhookSecretToken,
            StringComparison.Ordinal);
    }

    private static (string? ChatId, string? MessageText) ExtractMessage(JsonElement update)
    {
        if (!update.TryGetProperty("message", out var messageNode))
        {
            return (null, null);
        }

        if (!messageNode.TryGetProperty("chat", out var chatNode)
            || !chatNode.TryGetProperty("id", out var chatIdNode))
        {
            return (null, null);
        }

        var chatId = chatIdNode.ValueKind switch
        {
            JsonValueKind.Number when chatIdNode.TryGetInt64(out var numberValue)
                => numberValue.ToString(),
            JsonValueKind.String => chatIdNode.GetString(),
            _ => null
        };

        var messageText = messageNode.TryGetProperty("text", out var textNode)
            ? textNode.GetString()
            : null;

        return (chatId, messageText);
    }

}