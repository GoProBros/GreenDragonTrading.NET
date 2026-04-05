using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Users.Commands.UpdateMyProfile;

/// <summary>
/// Handler for UpdateMyProfileCommand.
/// </summary>
public class UpdateMyProfileCommandHandler : IRequestHandler<UpdateMyProfileCommand, ApiResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateMyProfileCommandHandler> _logger;

    public UpdateMyProfileCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        ILogger<UpdateMyProfileCommandHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != nameof(UserRole.User))
        {
            throw new AccessDeniedException("Chỉ người dùng có role User mới được cập nhật hồ sơ cá nhân.");
        }

        var userId = _currentUserService.GetRequiredUserId();

        var user = await _uow.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("Người dùng không tồn tại.");
        }

        if (user.Role != UserRole.User)
        {
            throw new AccessDeniedException("Chỉ người dùng có role User mới được cập nhật hồ sơ cá nhân.");
        }

        user.Username = request.FullName.Trim();
        user.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl)
            ? null
            : request.AvatarUrl.Trim();

        await _uow.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User profile updated successfully for user {UserId}",
            userId);

        return ApiResponse.Success("Cập nhật hồ sơ thành công.");
    }
}
