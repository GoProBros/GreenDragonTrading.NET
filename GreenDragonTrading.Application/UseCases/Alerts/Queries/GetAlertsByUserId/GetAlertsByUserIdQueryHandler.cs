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
    : IRequestHandler<GetAlertsByUserIdQuery, ApiResponse<List<AlertDto>>>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<ApiResponse<List<AlertDto>>> Handle(GetAlertsByUserIdQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new BusinessRuleException("UserId không hợp lệ.");
        }

        var currentUserId = _currentUserService.GetRequiredUserId();

        if (!_currentUserService.IsAdminOrStaff && request.UserId != currentUserId)
        {
            throw new AccessDeniedException("Bạn chỉ có thể xem cảnh báo của chính mình.");
        }

        var alerts = await _uow.Alerts.GetByUserIdAsync(request.UserId, cancellationToken);

        var data = alerts.Select(alert => new AlertDto
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
            UpdatedAt = alert.UpdatedAt
        }).ToList();

        return ApiResponse<List<AlertDto>>.Success(data, "Lấy danh sách cảnh báo thành công.");
    }
}
