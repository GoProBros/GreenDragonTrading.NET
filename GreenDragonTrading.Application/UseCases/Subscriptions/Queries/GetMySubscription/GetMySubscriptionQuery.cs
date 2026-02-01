using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Queries.GetMySubscription
{
    /// <summary>
    /// Query to get the current user's subscription information from token
    /// </summary>
    public record GetMySubscriptionQuery : IRequest<ApiResponse<UserSubscriptionDto>>;
}
