using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Queries.GetProactiveAlertLayerBSettings
{
    public class GetProactiveAlertLayerBSettingsQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetProactiveAlertLayerBSettingsQuery, ApiResponse<ProactiveAlertLayerBSettingsDto>>
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<ProactiveAlertLayerBSettingsDto>> Handle(
            GetProactiveAlertLayerBSettingsQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUserService.IsAdminOrStaff)
            {
                throw new AccessDeniedException("Only admin/staff can view layer B settings.");
            }

            var settings = await _uow.ProactiveAlertLayerBSettings.FirstOrDefaultAsync(
                _ => true,
                cancellationToken);

            if (settings == null)
            {
                var now = DateTimeOffset.UtcNow;
                settings = ProactiveAlertLayerBSetting.CreateDefault(now);
                await _uow.ProactiveAlertLayerBSettings.AddAsync(settings, cancellationToken);
                await _uow.SaveChangesAsync(cancellationToken);
            }

            return ApiResponse<ProactiveAlertLayerBSettingsDto>.Success(
                MapToDto(settings),
                "Layer B settings retrieved successfully.");
        }

        private static ProactiveAlertLayerBSettingsDto MapToDto(ProactiveAlertLayerBSetting settings)
        {
            return new ProactiveAlertLayerBSettingsDto
            {
                Id = settings.Id,
                Timeframe = settings.Timeframe,
                MinAbsoluteMovePercent = settings.MinAbsoluteMovePercent,
                AtrMoveMultiplier = settings.AtrMoveMultiplier,
                MinVolumeRatio = settings.MinVolumeRatio,
                MinAdx = settings.MinAdx,
                MaxIndicatorSnapshotAgeMinutes = settings.MaxIndicatorSnapshotAgeMinutes,
                ThrottleSeconds = settings.ThrottleSeconds,
                CooldownMinutes = settings.CooldownMinutes,
                CreatedAt = settings.CreatedAt,
                UpdatedAt = settings.UpdatedAt
            };
        }
    }
}
