using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Commands.ToggleAlertStatus
{
    public record ToggleAlertStatusCommand(int Id) : IRequest<ApiResponse<AlertDto>>;
}
