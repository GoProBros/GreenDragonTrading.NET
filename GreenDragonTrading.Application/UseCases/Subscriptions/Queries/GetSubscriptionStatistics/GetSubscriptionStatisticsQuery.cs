using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Queries.GetSubscriptionStatistics;

/// <summary>
/// Query for retrieving subscription and user statistics by year.
/// </summary>
/// <param name="Year">Target year for monthly statistics. If null, current year is used.</param>
public record GetSubscriptionStatisticsQuery(int? Year = null) : IRequest<ApiResponse<SubscriptionStatisticsDto>>;