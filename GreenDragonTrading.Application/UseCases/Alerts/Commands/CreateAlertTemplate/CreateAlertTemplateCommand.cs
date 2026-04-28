using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Commands.CreateAlertTemplate;

public record CreateAlertTemplateCommand(
    AlertType? Type,
    ConditionType? Condition,
    string TitleTemplate,
    string BodyTemplate,
    bool IsActive = true,
    bool IsDefault = false)
    : IRequest<ApiResponse<AlertTemplateDto>>;
