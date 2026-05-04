using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Queries.GetAlertsByUserId;

public record GetAlertsByUserIdQuery : PaginationQuery, IRequest<ApiResponse<PaginatedResponse<AlertDto>>>
{
	public Guid? UserId { get; init; }
	public AlertType? Type { get; init; }
	public ConditionType? Condition { get; init; }
}
