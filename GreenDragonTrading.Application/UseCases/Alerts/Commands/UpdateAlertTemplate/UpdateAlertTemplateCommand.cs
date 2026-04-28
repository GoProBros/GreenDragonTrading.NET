using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Commands.UpdateAlertTemplate;

public record UpdateAlertTemplateCommand(
    int Id,
    AlertType? Type,
    ConditionType? Condition,
    string TitleTemplate,
    string BodyTemplate,
    bool IsActive,
    bool IsDefault)
    : IRequest<ApiResponse<AlertTemplateDto>>;
