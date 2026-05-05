using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Queries.GetProactiveAlertLayerBSettings
{
    /// <summary>
    /// Retrieves the current global layer B settings for proactive alerts.
    /// </summary>
    public record GetProactiveAlertLayerBSettingsQuery
        : IRequest<ApiResponse<ProactiveAlertLayerBSettingsDto>>;
}
