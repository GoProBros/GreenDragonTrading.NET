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

namespace GreenDragonTrading.Application.UseCases.ModuleLayouts.Commands.UpdateLayout;

/// <summary>
/// Handler cho UpdateLayoutCommand
/// </summary>
public class UpdateLayoutCommandHandler : IRequestHandler<UpdateLayoutCommand, ApiResponse<ModuleLayoutDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtService _jwtService;
    private readonly ILogger<UpdateLayoutCommandHandler> _logger;

    public UpdateLayoutCommandHandler(
        IUnitOfWork uow,
        IHttpContextAccessor httpContextAccessor,
        IJwtService jwtService,
        ILogger<UpdateLayoutCommandHandler> logger)
    {
        _uow = uow;
        _httpContextAccessor = httpContextAccessor;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<ApiResponse<ModuleLayoutDto>> Handle(
        UpdateLayoutCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Lấy access token từ Authorization header
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

            // Sử dụng JwtService để lấy thông tin từ token
            var tokenInfo = _jwtService.GetTokenInfo(accessToken);
            if (tokenInfo == null)
            {
                throw new UnauthenticatedException("Access token không hợp lệ.");
            }

            // Lấy thông tin user từ database
            var user = await _uow.Users.GetByIdAsync(tokenInfo.UserId, cancellationToken);
            if (user == null)
            {
                throw new NotFoundException("Người dùng không tồn tại.");
            }

            // Lấy layout
            var layout = await _uow.ModuleLayouts.GetByIdAsync(request.Id, cancellationToken);

            if (layout == null)
            {
                return ApiResponse<ModuleLayoutDto>.Failure(
                    "Không tìm thấy layout. Layout không tồn tại.");
            }

            // Kiểm tra quyền (chỉ owner mới được update)
            if (layout.UserId.HasValue && layout.UserId.Value != user.Id)
            {
                return ApiResponse<ModuleLayoutDto>.Failure(
                    "Không có quyền. Bạn không có quyền cập nhật layout này.");
            }

            // Nếu là system layout, không được update
            if (layout.IsSystemDefault && layout.UserId == null)
            {
                return ApiResponse<ModuleLayoutDto>.Failure(
                    "Không có quyền. Không thể cập nhật layout hệ thống.");
            }

            // Update các field

            layout.LayoutName =(!string.IsNullOrWhiteSpace(request.LayoutName) ? request.LayoutName.Trim() : layout.LayoutName);
            layout.ConfigJson = (request.ConfigJson.HasValue) ? JsonSerializer.Serialize(request.ConfigJson.Value) : layout.ConfigJson;
            layout.IsSystemDefault = (request.IsSystemDefault.HasValue) ? request.IsSystemDefault.Value : layout.IsSystemDefault;
            layout.UpdatedAt = DateTimeOffset.UtcNow;

            await _uow.SaveChangesAsync(cancellationToken);

            // Parse JSON config for response
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

            _logger.LogInformation("Layout updated successfully {LayoutId} for user {UserId}", layout.Id, user.Id);

            return ApiResponse<ModuleLayoutDto>.Success(result, "Cập nhật layout thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating layout {LayoutId}", request.Id);
            throw;
        }
    }
}
