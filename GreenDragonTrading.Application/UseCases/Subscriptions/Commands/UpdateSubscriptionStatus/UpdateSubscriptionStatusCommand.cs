using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Commands.UpdateSubscriptionStatus
{
    public record UpdateSubscriptionStatusCommand(
        int SubscriptionId
    ) : IRequest<ApiResponse<SubscriptionDto>>;
}
