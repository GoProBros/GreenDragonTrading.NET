using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Commands.CreateSubscription
{
    public record CreateSubscriptionCommand(
        string Name,
        SubscriptionLevel LevelOrder,
        int MaxWorkspaces,
        decimal Price,
        int DurationInDays,
        JsonElement AllowedModules,
        CommonStatus IsActive = CommonStatus.Active
    ) : IRequest<ApiResponse<SubscriptionDto>>;
}
