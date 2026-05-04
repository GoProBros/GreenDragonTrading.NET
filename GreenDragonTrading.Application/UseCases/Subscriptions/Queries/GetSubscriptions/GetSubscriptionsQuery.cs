using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Queries.GetSubscriptions
{
    public record GetSubscriptionsQuery : IRequest<ApiResponse<List<SubscriptionDto>>>;
}
