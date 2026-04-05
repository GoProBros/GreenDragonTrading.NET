using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Commands.UpdateSubscriptionStatus
{
    public record UpdateSubscriptionStatusCommand(
        int SubscriptionId,
        CommonStatus IsActive
    ) : IRequest<ApiResponse<SubscriptionDto>>;
}
