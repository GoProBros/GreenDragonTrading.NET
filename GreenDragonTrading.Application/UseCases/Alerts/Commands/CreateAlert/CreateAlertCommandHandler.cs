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

            var currentMonitoredValue = await GetCurrentMonitoredValueAsync(request.Type, ticker);
            var calculatedThreshold = CalculateThresholdValue(request, currentMonitoredValue);

            var alert = new Alert
            {
                UserId = userId,
                Ticker = ticker,
                Type = request.Type,
                Condition = request.Condition,
                ChangePercentage = request.Condition is ConditionType.Above or ConditionType.Below
                    ? null
                    : request.ChangePercentage,
                CurrentPrice = currentMonitoredValue,
                ThresholdValue = calculatedThreshold,
                Name = request.Name?.Trim(),
                IsActive = request.IsActive,
                IsTriggered = false,
                ChatSessionId = request.ChatSessionId,
                MessageTemplate = string.IsNullOrWhiteSpace(request.MessageTemplate)
                    ? null
                    : request.MessageTemplate.Trim(),
                NotifyVia = NotificationChannel.System,
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

        private async Task<decimal> GetCurrentMonitoredValueAsync(AlertType type, string ticker)
        {
            var marketData = await _redisService.GetHashAsync<MarketSymbolDto>(
                RedisConstants.MarketDataSymbol(ticker));

            if (marketData == null)
            {
                throw new BusinessRuleException("Không tìm thấy dữ liệu thị trường hiện tại trên Redis");
            }

            var monitoredValue = type == AlertType.Price
                ? (decimal)marketData.LastPrice
                : (decimal)marketData.TotalVol;

            if (monitoredValue <= 0)
            {
                throw new BusinessRuleException(
                    type == AlertType.Price
                        ? "Giá hiện tại chưa sẵn sàng để đặt cảnh báo"
                        : "Khối lượng hiện tại chưa sẵn sàng để đặt cảnh báo");
            }

            return monitoredValue;
        }

        private static decimal CalculateThresholdValue(CreateAlertCommand request, decimal currentMonitoredValue)
        {
            if (request.Condition is ConditionType.Above or ConditionType.Below)
            {
                if (!request.ThresholdValue.HasValue)
                {
                    throw new BusinessRuleException("Condition 1,2 yêu cầu thresholdValue");
                }

                return request.ThresholdValue.Value;
            }

            if (!request.ChangePercentage.HasValue)
            {
                throw new BusinessRuleException("Condition 3,4 yêu cầu changePercentage");
            }

            var pct = request.ChangePercentage.Value;
            var factor = pct / 100m;

            return request.Condition switch
            {
                ConditionType.PercentChangeUp => currentMonitoredValue * (1m + factor),
                ConditionType.PercentChangeDown => currentMonitoredValue * (1m - factor),
                _ => throw new BusinessRuleException("Condition không hợp lệ"),
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
        }
    }
}
