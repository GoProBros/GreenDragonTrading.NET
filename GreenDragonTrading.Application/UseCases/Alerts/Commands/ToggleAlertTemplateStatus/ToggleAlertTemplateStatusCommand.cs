using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Commands.ToggleAlertTemplateStatus;

public record ToggleAlertTemplateStatusCommand(int Id)
    : IRequest<ApiResponse<AlertTemplateDto>>;
