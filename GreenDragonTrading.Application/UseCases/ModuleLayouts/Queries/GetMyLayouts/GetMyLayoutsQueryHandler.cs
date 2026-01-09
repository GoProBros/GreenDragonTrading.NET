using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace GreenDragonTrading.Application.UseCases.ModuleLayouts.Queries.GetMyLayouts;

/// <summary>
/// Handler cho GetMyLayoutsQuery
/// </summary>
public class GetMyLayoutsQueryHandler : IRequestHandler<GetMyLayoutsQuery, ApiResponse<List<ModuleLayoutListItemDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtService _jwtService;
    private readonly ILogger<GetMyLayoutsQueryHandler> _logger;

    public GetMyLayoutsQueryHandler(
        IUnitOfWork uow,
        IHttpContextAccessor httpContextAccessor,
        IJwtService jwtService,
        ILogger<GetMyLayoutsQueryHandler> logger)
    {
        _uow = uow;
        _httpContextAccessor = httpContextAccessor;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<ApiResponse<List<ModuleLayoutListItemDto>>> Handle(
        GetMyLayoutsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new UnauthenticatedException("Không tìm thấy HTTP context.");
            }

            var authHeaderValue = httpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeaderValue) || !authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthenticatedException("Không tìm thấy Authorization header.");
            }

            var accessToken = authHeaderValue.Substring("Bearer ".Length).Trim();

            var tokenInfo = _jwtService.GetTokenInfo(accessToken);
            if (tokenInfo == null)
            {
                throw new UnauthenticatedException("Access token không hợp lệ.");
            }

            var user = await _uow.Users.GetByIdAsync(tokenInfo.UserId, cancellationToken);
            if (user == null)
            {
                throw new NotFoundException("Người dùng không tồn tại.");
            }

            // Lấy layouts (system + personal của user)
            var layouts = await _uow.ModuleLayouts.GetByModuleTypeAsync(
                request.ModuleType,
                user.Id,
                cancellationToken);

            var result = layouts.Select(l => new ModuleLayoutListItemDto
            {
                Id = l.Id,
                LayoutName = l.LayoutName,
                ModuleType = l.ModuleType,
                ModuleTypeName = l.ModuleType.GetDisplayName(),
                IsSystemDefault = l.IsSystemDefault,
                IsPersonal = l.UserId.HasValue,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            }).ToList();

            _logger.LogInformation("Successfully retrieved {Count} layouts for module type {ModuleType} for user {UserId}",
                result.Count, request.ModuleType, user.Id);

            return ApiResponse<List<ModuleLayoutListItemDto>>.Success(
                result,
                "Lấy danh sách layout thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving layouts for module type {ModuleType}", request.ModuleType);
            throw;
        }
    }
}
