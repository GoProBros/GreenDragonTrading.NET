using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Queries.GetSubscriptionStatistics;

/// <summary>
/// Query for retrieving subscription and user statistics.
/// </summary>
public record GetSubscriptionStatisticsQuery : IRequest<ApiResponse<SubscriptionStatisticsDto>>;