using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Payments.Commands.CreatePaymentLink
{
    public record CreatePaymentLinkCommand(int SubscriptionId) : IRequest<ApiResponse<PaymentLinkResponse>>;
}
