using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Commands.ToggleAlertStatus
{
    public class ToggleAlertStatusCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        IRedisService redisService) : IRequestHandler<ToggleAlertStatusCommand, ApiResponse<AlertDto>>
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IRedisService _redisService = redisService;

        public async Task<ApiResponse<AlertDto>> Handle(ToggleAlertStatusCommand request, CancellationToken cancellationToken)
        {
            if (request.Id <= 0)
            {
                throw new BusinessRuleException("Id cảnh báo không hợp lệ.");
            }

            var userId = _currentUserService.GetRequiredUserId();
            var alert = await _uow.Alerts.GetByIdAsync(request.Id, cancellationToken);
            if (alert == null || alert.UserId != userId)
            {
                throw new NotFoundException("Không tìm thấy cảnh báo");
            }

            alert.IsActive = !alert.IsActive;
            alert.UpdatedAt = DateTimeOffset.UtcNow;
            _uow.Alerts.Update(alert);

            var redisKey = RedisConstants.AlertsByTypeAndCondition(alert.Ticker, alert.Type, alert.Condition);
            if (alert.IsActive && !alert.IsTriggered)
            {
                await _redisService.SortedSetAddAsync(redisKey, alert.Id.ToString(), (double)alert.ThresholdValue);
            }
            else
            {
                await _redisService.SortedSetRemoveAsync(redisKey, alert.Id.ToString());
                // Backward-compatible cleanup for old redis key layout.
                await _redisService.SortedSetRemoveAsync(RedisConstants.AlertsAbove(alert.Ticker), alert.Id.ToString());
                await _redisService.SortedSetRemoveAsync(RedisConstants.AlertsBelow(alert.Ticker), alert.Id.ToString());
            }

            await _uow.SaveChangesAsync(cancellationToken);

            var dto = new AlertDto
            {
                Id = alert.Id,
                UserId = alert.UserId,
                Ticker = alert.Ticker,
                Type = alert.Type,
                Condition = alert.Condition,
                ChangePercentage = alert.ChangePercentage,
                ThresholdValue = alert.ThresholdValue,
                Name = alert.Name,
                IsActive = alert.IsActive,
                IsTriggered = alert.IsTriggered,
                LastTriggeredAt = alert.LastTriggeredAt,
                ChatSessionId = alert.ChatSessionId,
                MessageTemplate = alert.MessageTemplate,
                CreatedAt = alert.CreatedAt,
                UpdatedAt = alert.UpdatedAt,
            };

            var statusText = alert.IsActive ? "bật" : "tắt";
            return ApiResponse<AlertDto>.Success(dto, $"Chuyển trạng thái cảnh báo thành công ({statusText}).");
        }
    }
}
