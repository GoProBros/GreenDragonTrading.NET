using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Queries.GetAlertTemplatePlaceholders;

public record GetAlertTemplatePlaceholdersQuery : IRequest<ApiResponse<AlertTemplatePlaceholdersDto>>;
