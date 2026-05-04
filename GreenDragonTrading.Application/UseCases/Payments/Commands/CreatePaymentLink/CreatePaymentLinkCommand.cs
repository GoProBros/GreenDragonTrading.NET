using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Payments.Commands.CreatePaymentLink
{
    /// <summary>
    /// Creates a payment link for the given subscription via the chosen provider (PayOS or Momo).
    /// </summary>
    public record CreatePaymentLinkCommand(
        int SubscriptionId,
        PaymentType PaymentProvider) : IRequest<ApiResponse<PaymentLinkResponse>>;
}
