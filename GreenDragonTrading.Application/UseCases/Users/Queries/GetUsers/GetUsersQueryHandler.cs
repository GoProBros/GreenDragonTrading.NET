using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Users.Queries.GetUsers;

/// <summary>
/// Handler for GetUsersQuery.
/// </summary>
public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, ApiResponse<PaginatedResponse<UserManagementListItemDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetUsersQueryHandler> _logger;

    public GetUsersQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        ILogger<GetUsersQueryHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<PaginatedResponse<UserManagementListItemDto>>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdminOrStaff)
        {
            throw new AccessDeniedException("Bạn không có quyền truy cập danh sách người dùng.");
        }

        var role = _currentUserService.Role;
        var users = (await _uow.Users.GetAllAsync(cancellationToken)).ToList();

        if (role == nameof(UserRole.Staff))
        {
            users = users.Where(x => x.Role == UserRole.User).ToList();
        }
        else if (role == nameof(UserRole.Admin))
        {
            users = users.Where(x => x.Role == UserRole.User || x.Role == UserRole.Staff).ToList();
        }

        if (request.Role.HasValue)
        {
            users = users.Where(x => x.Role == request.Role.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim();
            users = users.Where(x =>
                    x.Username.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || x.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || x.PhoneNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var mappedUsers = users
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new UserManagementListItemDto
            {
                Id = x.Id,
                Name = x.Username,
                Role = x.Role.GetDisplayName(),
                Phone = x.PhoneNumber,
                Email = x.Email,
                Avatar = x.AvatarUrl,
                Status = x.Status == CommonStatus.Active ? "Active" : "Inactive"
            })
            .ToList();

        var totalCount = mappedUsers.Count;
        var pagedItems = mappedUsers
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var paginatedResponse = PaginatedResponse<UserManagementListItemDto>.Create(
            pagedItems,
            totalCount,
            request.PageIndex,
            request.PageSize);

        _logger.LogInformation(
            "Retrieved users page {PageIndex}/{PageSize} for role {Role}, filterRole={FilterRole}, search={Search}, total {TotalCount}",
            request.PageIndex,
            request.PageSize,
            role,
            request.Role,
            request.Search,
            totalCount);

        return ApiResponse<PaginatedResponse<UserManagementListItemDto>>.Success(
            paginatedResponse,
            "Lấy danh sách người dùng thành công.");
    }
}