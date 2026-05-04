using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Queries.GetAlertTemplates;

public record GetAlertTemplatesQuery(
    AlertType? Type = null,
    ConditionType? Condition = null,
    bool? IsActive = null,
    bool? IsDefault = null)
    : IRequest<ApiResponse<List<AlertTemplateDto>>>;
