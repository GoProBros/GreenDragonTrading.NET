using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Users.Commands.ActivateUser;

/// <summary>
/// Handler for ActivateUserCommand.
/// </summary>
public class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand, ApiResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ActivateUserCommandHandler> _logger;

    public ActivateUserCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        ILogger<ActivateUserCommandHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
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

        // Admin can manage Staff/User; Staff can manage User only.
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

        if (targetUser.Status == CommonStatus.Active)
        {
            return ApiResponse.Success("Tài khoản đã ở trạng thái Active.");
        }

        targetUser.Status = CommonStatus.Active;
        await _uow.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {TargetUserId} was activated by {ActorUserId} with role {ActorRole}",
            targetUser.Id,
            _currentUserService.UserId,
            actorRole);

        return ApiResponse.Success("Đổi trạng thái người dùng thành Active thành công.");
    }
}
