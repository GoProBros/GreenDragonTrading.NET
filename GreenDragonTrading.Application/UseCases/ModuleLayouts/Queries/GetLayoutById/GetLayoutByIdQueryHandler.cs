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
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.ModuleLayouts.Queries.GetLayoutById;

/// <summary>
/// Handler cho GetLayoutByIdQuery
/// </summary>
public class GetLayoutByIdQueryHandler : IRequestHandler<GetLayoutByIdQuery, ApiResponse<ModuleLayoutDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtService _jwtService;
    private readonly ILogger<GetLayoutByIdQueryHandler> _logger;

    public GetLayoutByIdQueryHandler(
        IUnitOfWork uow,
        IHttpContextAccessor httpContextAccessor,
        IJwtService jwtService,
        ILogger<GetLayoutByIdQueryHandler> logger)
    {
        _uow = uow;
        _httpContextAccessor = httpContextAccessor;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<ApiResponse<ModuleLayoutDto>> Handle(
        GetLayoutByIdQuery request,
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

            var layout = await _uow.ModuleLayouts.GetByIdAndUserIdAsync(request.Id, user.Id, cancellationToken);

            if (layout == null)
            {
                return ApiResponse<ModuleLayoutDto>.Failure(
                    "Không tìm thấy layout. Layout không tồn tại hoặc bạn không có quyền truy cập.");
            }

            JsonElement? configJson = null;
            if (!string.IsNullOrWhiteSpace(layout.ConfigJson))
            {
                try
                {
                    configJson = JsonSerializer.Deserialize<JsonElement>(layout.ConfigJson);
                }
                catch
                {
                    configJson = null;
                }
            }

            var result = new ModuleLayoutDto
            {
                Id = layout.Id,
                LayoutName = layout.LayoutName,
                ModuleType = layout.ModuleType,
                ModuleTypeName = layout.ModuleType.GetDisplayName(),
                ConfigJson = configJson,
                IsSystemDefault = layout.IsSystemDefault,
                IsPersonal = layout.UserId.HasValue,
                UserId = layout.UserId,
                CreatedAt = layout.CreatedAt,
                UpdatedAt = layout.UpdatedAt
            };

            _logger.LogInformation("Layout retrieved successfully {LayoutId} for user {UserId}", request.Id, user.Id);

            return ApiResponse<ModuleLayoutDto>.Success(result, "Lấy layout thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving layout {LayoutId}", request.Id);
            throw;
        }
    }
}
