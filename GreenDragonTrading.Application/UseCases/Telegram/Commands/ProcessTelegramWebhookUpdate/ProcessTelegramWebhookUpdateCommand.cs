using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Telegram.Commands.ProcessTelegramWebhookUpdate;

public record ProcessTelegramWebhookUpdateCommand(string? ChatId, string? MessageText)
    : IRequest<ApiResponse>;