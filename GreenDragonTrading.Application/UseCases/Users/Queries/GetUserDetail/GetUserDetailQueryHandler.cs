using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Users.Queries.GetUserDetail;

/// <summary>
/// Handler for GetUserDetailQuery.
/// </summary>
public class GetUserDetailQueryHandler : IRequestHandler<GetUserDetailQuery, ApiResponse<UserManagementDetailDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetUserDetailQueryHandler> _logger;

    public GetUserDetailQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        ILogger<GetUserDetailQueryHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<UserManagementDetailDto>> Handle(
        GetUserDetailQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdminOrStaff)
        {
            throw new AccessDeniedException("Bạn không có quyền xem chi tiết người dùng.");
        }

        var user = await _uow.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("Người dùng không tồn tại.");
        }

        if (_currentUserService.Role == nameof(UserRole.Staff) && user.Role != UserRole.User)
        {
            throw new AccessDeniedException("Bạn không có quyền xem người dùng này.");
        }

        var result = new UserManagementDetailDto
        {
            Id = user.Id,
            Name = user.Username,
            Role = user.Role.GetDisplayName(),
            Phone = user.PhoneNumber,
            Email = user.Email,
            Avatar = user.AvatarUrl,
            Status = user.Status == CommonStatus.Active ? "Active" : "Inactive"
        };

        _logger.LogInformation("Retrieved detail for user {UserId} by role {Role}", request.UserId, _currentUserService.Role);

        return ApiResponse<UserManagementDetailDto>.Success(result, "Lấy thông tin người dùng thành công.");
    }
}