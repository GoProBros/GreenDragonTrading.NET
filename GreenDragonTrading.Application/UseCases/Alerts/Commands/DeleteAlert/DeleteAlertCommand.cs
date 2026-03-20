using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Commands.DeleteAlert
{
    public record DeleteAlertCommand(int Id) : IRequest<ApiResponse>;
}
