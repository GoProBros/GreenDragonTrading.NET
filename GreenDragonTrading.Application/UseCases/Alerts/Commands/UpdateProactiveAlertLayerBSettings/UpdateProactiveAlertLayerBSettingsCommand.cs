using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Commands.UpdateProactiveAlertLayerBSettings
{
    /// <summary>
    /// Updates the global layer B settings for proactive alerts.
    /// </summary>
    public record UpdateProactiveAlertLayerBSettingsCommand(
        decimal MinAbsoluteMovePercent,
        decimal AtrMoveMultiplier,
        decimal MinVolumeRatio,
        decimal MinAdx,
        int MaxIndicatorSnapshotAgeMinutes)
        : IRequest<ApiResponse<ProactiveAlertLayerBSettingsDto>>;
}
