using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Commands.CreateAlert
{
    public class CreateAlertCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        IRedisService redisService) : IRequestHandler<CreateAlertCommand, ApiResponse<AlertDto>>
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IRedisService _redisService = redisService;

        public async Task<ApiResponse<AlertDto>> Handle(CreateAlertCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetRequiredUserId();
            var ticker = request.Ticker.Trim().ToUpperInvariant();

            var symbol = await _uow.Symbols.GetByIdAsync(ticker, cancellationToken);
            if (symbol == null)
            {
                throw new NotFoundException("Mã cổ phiếu không tồn tại");
            }

            var calculatedThreshold = CalculateThresholdValue(request);

            var alert = new Alert
            {
                UserId = userId,
                Ticker = ticker,
                Type = request.Type,
                Condition = request.Condition,
                ChangePercentage = request.ChangePercentage,
                CurrentPrice = request.CurrentPrice,
                ThresholdValue = calculatedThreshold,
                Name = request.Name?.Trim(),
                IsActive = request.IsActive,
                IsTriggered = false,
                ChatSessionId = request.ChatSessionId,
                MessageTemplate = request.MessageTemplate,
                NotifyVia = request.NotifyVia,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            };

            await _uow.Alerts.AddAsync(alert, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            if (alert.IsActive && !alert.IsTriggered)
            {
                await _redisService.SortedSetAddAsync(
                    GetRedisKey(alert),
                    alert.Id.ToString(),
                    (double)alert.ThresholdValue);
            }

            var dto = MapToDto(alert);
            return ApiResponse<AlertDto>.Success(dto, "Đặt cảnh báo thành công");
        }

        private static string GetRedisKey(Alert alert)
        {
            return RedisConstants.AlertsByTypeAndCondition(alert.Ticker, alert.Type, alert.Condition);
        }

        private static decimal CalculateThresholdValue(CreateAlertCommand request)
        {
            if (request.Condition is ConditionType.Above or ConditionType.Below)
            {
                return request.ThresholdValue;
            }

            if (request.Type == AlertType.Volume)
            {
                return request.ThresholdValue;
            }

            var pct = request.ChangePercentage ?? 0m;
            var factor = pct / 100m;

            return request.Condition switch
            {
                ConditionType.PercentChangeUp => request.CurrentPrice * (1m + factor),
                ConditionType.PercentChangeDown => request.CurrentPrice * (1m - factor),
                _ => request.ThresholdValue,
            };
        }

        private static AlertDto MapToDto(Alert alert)
        {
            return new AlertDto
            {
                Id = alert.Id,
                UserId = alert.UserId,
                Ticker = alert.Ticker,
                Type = alert.Type,
                Condition = alert.Condition,
                ChangePercentage = alert.ChangePercentage,
                CurrentPrice = alert.CurrentPrice,
                ThresholdValue = alert.ThresholdValue,
                Name = alert.Name,
                IsActive = alert.IsActive,
                IsTriggered = alert.IsTriggered,
                LastTriggeredAt = alert.LastTriggeredAt,
                ChatSessionId = alert.ChatSessionId,
                MessageTemplate = alert.MessageTemplate,
                NotifyVia = alert.NotifyVia,
                CreatedAt = alert.CreatedAt,
                UpdatedAt = alert.UpdatedAt,
            };
        }
    }
}
