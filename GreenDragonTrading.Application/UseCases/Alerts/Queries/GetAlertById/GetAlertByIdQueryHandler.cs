using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Queries.GetAlertById
{
    public class GetAlertByIdQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService) : IRequestHandler<GetAlertByIdQuery, ApiResponse<AlertDto>>
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<ApiResponse<AlertDto>> Handle(GetAlertByIdQuery request, CancellationToken cancellationToken)
        {
            if (request.Id <= 0)
            {
                throw new BusinessRuleException("Id cảnh báo không hợp lệ.");
            }

            var currentUserId = _currentUserService.GetRequiredUserId();
            var alert = await _uow.Alerts.GetByIdAsync(request.Id, cancellationToken);
            if (alert == null)
            {
                throw new NotFoundException("Không tìm thấy cảnh báo.");
            }

            if (!_currentUserService.IsAdminOrStaff && alert.UserId != currentUserId)
            {
                throw new AccessDeniedException("Bạn không có quyền xem cảnh báo này.");
            }

            var dto = new AlertDto
            {
                Id = alert.Id,
                UserId = alert.UserId,
                Ticker = alert.Ticker,
                Type = alert.Type,
                Condition = alert.Condition,
                ChangePercentage = alert.ChangePercentage,
                ThresholdValue = alert.ThresholdValue,
                VolumeTimeFrame = alert.VolumeTimeFrame,
                VolumeLookbackBars = alert.VolumeLookbackBars,
                Name = alert.Name,
                IsActive = alert.IsActive,
                IsTriggered = alert.IsTriggered,
                LastTriggeredAt = alert.LastTriggeredAt,
                ChatSessionId = alert.ChatSessionId,
                MessageTemplate = alert.MessageTemplate,
                CreatedAt = alert.CreatedAt,
                UpdatedAt = alert.UpdatedAt,
            };

            return ApiResponse<AlertDto>.Success(dto, "Lấy chi tiết cảnh báo thành công.");
        }
    }
}
