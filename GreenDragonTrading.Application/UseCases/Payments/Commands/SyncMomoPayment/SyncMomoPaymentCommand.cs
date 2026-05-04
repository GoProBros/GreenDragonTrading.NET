using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Payments.Commands.SyncMomoPayment
{
    public record SyncMomoPaymentCommand(long OrderCode) : IRequest<ApiResponse<WebhookUpdateResult>>;
}
