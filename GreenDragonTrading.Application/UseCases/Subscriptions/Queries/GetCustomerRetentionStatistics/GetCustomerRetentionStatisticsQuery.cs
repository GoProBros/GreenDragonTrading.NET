using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Queries.GetCustomerRetentionStatistics;

/// <summary>
/// Query for retrieving customer retention statistics.
/// </summary>
public record GetCustomerRetentionStatisticsQuery : IRequest<ApiResponse<CustomerRetentionStatisticsDto>>;
