using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Payments.Commands.CancelPaymentLink
{
    public record CancelPaymentLinkCommand(long orderCode, string reason) : IRequest<ApiResponse<PaymentInformationResponse>>;

}
