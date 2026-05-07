using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Commands.UpdateProactiveAlertLayerBSettings
{
    public class UpdateProactiveAlertLayerBSettingsCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        IRedisService redisService)
        : IRequestHandler<UpdateProactiveAlertLayerBSettingsCommand, ApiResponse<ProactiveAlertLayerBSettingsDto>>
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IRedisService _redisService = redisService;

        public async Task<ApiResponse<ProactiveAlertLayerBSettingsDto>> Handle(
            UpdateProactiveAlertLayerBSettingsCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUserService.IsAdminOrStaff)
            {
                throw new AccessDeniedException("Only admin/staff can update layer B settings.");
            }

            var now = DateTimeOffset.UtcNow;

            var settings = await _uow.ProactiveAlertLayerBSettings.FirstOrDefaultAsync(
                _ => true,
                cancellationToken);

            if (settings == null)
            {
                settings = new ProactiveAlertLayerBSetting
                {
                    Timeframe = ProactiveAlertLayerBDefaults.Timeframe,
                    MinAbsoluteMovePercent = request.MinAbsoluteMovePercent,
                    AtrMoveMultiplier = request.AtrMoveMultiplier,
                    MinVolumeRatio = request.MinVolumeRatio,
                    MinAdx = request.MinAdx,
                    MaxIndicatorSnapshotAgeMinutes = request.MaxIndicatorSnapshotAgeMinutes,
                    ThrottleSeconds = request.ThrottleSeconds,
                    CooldownMinutes = request.CooldownMinutes,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await _uow.ProactiveAlertLayerBSettings.AddAsync(settings, cancellationToken);
            }
            else
            {
                settings.MinAbsoluteMovePercent = request.MinAbsoluteMovePercent;
                settings.AtrMoveMultiplier = request.AtrMoveMultiplier;
                settings.MinVolumeRatio = request.MinVolumeRatio;
                settings.MinAdx = request.MinAdx;
                settings.MaxIndicatorSnapshotAgeMinutes = request.MaxIndicatorSnapshotAgeMinutes;
                settings.ThrottleSeconds = request.ThrottleSeconds;
                settings.CooldownMinutes = request.CooldownMinutes;
                settings.UpdatedAt = now;

                _uow.ProactiveAlertLayerBSettings.Update(settings);
            }

            await _uow.SaveChangesAsync(cancellationToken);
            await _redisService.RemoveAsync(RedisConstants.ProactiveAlertLayerBSettings());

            return ApiResponse<ProactiveAlertLayerBSettingsDto>.Success(
                MapToDto(settings),
                "Layer B settings updated successfully.");
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
