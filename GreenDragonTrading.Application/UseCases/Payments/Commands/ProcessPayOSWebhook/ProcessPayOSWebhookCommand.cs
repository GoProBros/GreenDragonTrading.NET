using GreenDragonTrading.Application.Common.Models;
using MediatR;
using Net.payOS.Types;

namespace GreenDragonTrading.Application.UseCases.Payments.Commands.ProcessPayOSWebhook
{
    public record ProcessPayOSWebhookCommand(WebhookType WebhookBody) : IRequest<ApiResponse>;
}
