using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Commands.CreateSubscription
{
    public record CreateSubscriptionCommand(
        string Name,
        SubscriptionLevel LevelOrder,
        int MaxLayouts,
        decimal Price,
        int DurationInDays
    ) : IRequest<ApiResponse<SubscriptionDto>>;
}
