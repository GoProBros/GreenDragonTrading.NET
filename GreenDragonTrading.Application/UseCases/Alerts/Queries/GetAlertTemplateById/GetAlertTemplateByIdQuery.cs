using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Queries.GetAlertTemplateById;

public record GetAlertTemplateByIdQuery(int Id) : IRequest<ApiResponse<AlertTemplateDto>>;
