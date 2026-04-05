using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Commands.UpdateSubscriptionPrice
{
    public record UpdateSubscriptionPriceCommand(
        int SubscriptionId,
        decimal Price
    ) : IRequest<ApiResponse<SubscriptionDto>>;
}
