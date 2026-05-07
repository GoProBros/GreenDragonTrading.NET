using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Queries.GetAlertsByUserId;

public class GetAlertsByUserIdQueryHandler(
    IUnitOfWork uow,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetAlertsByUserIdQuery, ApiResponse<PaginatedResponse<AlertDto>>>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<ApiResponse<PaginatedResponse<AlertDto>>> Handle(GetAlertsByUserIdQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId.HasValue && request.UserId.Value == Guid.Empty)
        {
            throw new BusinessRuleException("UserId không hợp lệ.");
        }

        var currentUserId = _currentUserService.GetRequiredUserId();
        var targetUserId = request.UserId ?? currentUserId;

        if (!_currentUserService.IsAdminOrStaff && targetUserId != currentUserId)
        {
            throw new AccessDeniedException("Bạn chỉ có thể xem cảnh báo của chính mình.");
        }

        var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var (alerts, totalCount) = await _uow.Alerts.GetPaginatedByUserIdAsync(
            targetUserId,
            request.Type,
            request.Condition,
            pageIndex,
            pageSize,
            cancellationToken);

        var data = alerts.Select(alert => new AlertDto
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
            UpdatedAt = alert.UpdatedAt
        }).ToList();

        var paginatedResponse = PaginatedResponse<AlertDto>.Create(data, totalCount, pageIndex, pageSize);

        return ApiResponse<PaginatedResponse<AlertDto>>.Success(
            paginatedResponse,
            "Lấy danh sách cảnh báo thành công.");
    }
}
