using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Queries.GetAlertsByUserId;

public record GetAlertsByUserIdQuery(Guid UserId) : IRequest<ApiResponse<List<AlertDto>>>;
