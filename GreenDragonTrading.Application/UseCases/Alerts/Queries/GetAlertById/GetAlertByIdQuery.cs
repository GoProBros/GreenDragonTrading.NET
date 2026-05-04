using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Queries.GetAlertById
{
    public record GetAlertByIdQuery(int Id) : IRequest<ApiResponse<AlertDto>>;
}
