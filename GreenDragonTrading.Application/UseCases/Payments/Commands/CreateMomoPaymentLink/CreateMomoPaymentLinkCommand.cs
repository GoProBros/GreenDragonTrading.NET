using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Payments.Commands.CreateMomoPaymentLink
{
    /// <summary>
    /// Creates a Momo payment link for a given subscription.
    /// Returns the Momo checkout URL and the generated orderId.
    /// </summary>
    public record CreateMomoPaymentLinkCommand(int SubscriptionId) : IRequest<ApiResponse<MomoPaymentLinkResponse>>;
}
