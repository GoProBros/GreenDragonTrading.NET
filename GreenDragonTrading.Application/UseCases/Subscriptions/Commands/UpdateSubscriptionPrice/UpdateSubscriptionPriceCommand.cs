using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Commands.UpdateSubscriptionPrice
{
    public record UpdateSubscriptionPriceCommand(
        int SubscriptionId,
        decimal? Price,
        JsonElement? AllowedModules
    ) : IRequest<ApiResponse<SubscriptionDto>>;
}
