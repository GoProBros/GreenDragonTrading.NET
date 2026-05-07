using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Users.Commands.ToggleUserStatus;

/// <summary>
/// Handler for ToggleUserStatusCommand.
/// </summary>
public class ToggleUserStatusCommandHandler : IRequestHandler<ToggleUserStatusCommand, ApiResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ToggleUserStatusCommandHandler> _logger;

    public ToggleUserStatusCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        ILogger<ToggleUserStatusCommandHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse> Handle(ToggleUserStatusCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdminOrStaff)
        {
            throw new AccessDeniedException("Bạn không có quyền cập nhật trạng thái người dùng.");
        }

        var targetUser = await _uow.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (targetUser == null)
        {
            throw new NotFoundException("Người dùng không tồn tại.");
        }

        var actorRole = _currentUserService.Role;

        if (actorRole == nameof(UserRole.Admin))
        {
            if (targetUser.Role == UserRole.Admin)
            {
                throw new AccessDeniedException("Không thể cập nhật trạng thái tài khoản Admin.");
            }
        }
        else if (actorRole == nameof(UserRole.Staff))
        {
            if (targetUser.Role != UserRole.User)
            {
                throw new AccessDeniedException("Staff chỉ có quyền cập nhật trạng thái tài khoản User.");
            }
        }

        targetUser.Status = targetUser.Status == CommonStatus.Active
            ? CommonStatus.InActive
            : CommonStatus.Active;

        await _uow.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {TargetUserId} status was toggled to {TargetStatus} by {ActorUserId} with role {ActorRole}",
            targetUser.Id,
            targetUser.Status,
            _currentUserService.UserId,
            _currentUserService.Role);

        var statusText = targetUser.Status == CommonStatus.Active ? "Active" : "Inactive";
        return ApiResponse.Success($"Đổi trạng thái người dùng thành {statusText} thành công.");
    }
}
