using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.ModuleLayouts.Commands.CreateLayout;

public class CreateLayoutCommandHandler : IRequestHandler<CreateLayoutCommand, ApiResponse<ModuleLayoutDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtService _jwtService;
    private readonly ILogger<CreateLayoutCommandHandler> _logger;

    public CreateLayoutCommandHandler(
        IUnitOfWork uow,
        IHttpContextAccessor httpContextAccessor,
        IJwtService jwtService,
        ILogger<CreateLayoutCommandHandler> logger)
    {
        _uow = uow;
        _httpContextAccessor = httpContextAccessor;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<ApiResponse<ModuleLayoutDto>> Handle(
        CreateLayoutCommand request,
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

            var configJsonString = JsonSerializer.Serialize(request.ConfigJson);

            var layout = new ModuleLayout
            {
                LayoutName = request.LayoutName.Trim(),
                ModuleType = request.ModuleType,
                ConfigJson = configJsonString,
                IsSystemDefault = request.IsSystemDefault,
                UserId = request.IsSystemDefault ? null : user.Id,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _uow.ModuleLayouts.AddAsync(layout, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            var result = new ModuleLayoutDto
            {
                Id = layout.Id,
                LayoutName = layout.LayoutName,
                ModuleType = layout.ModuleType,
                ModuleTypeName = layout.ModuleType.GetDisplayName(),
                ConfigJson = request.ConfigJson,
                IsSystemDefault = layout.IsSystemDefault,
                IsPersonal = layout.UserId.HasValue,
                UserId = layout.UserId,
                CreatedAt = layout.CreatedAt,
                UpdatedAt = layout.UpdatedAt
            };

            _logger.LogInformation("Layout created successfully {LayoutId} for user {UserId}", layout.Id, user.Id);

            return ApiResponse<ModuleLayoutDto>.Success(result, "Tạo layout mới thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating layout");
            throw;
        }
    }
}
