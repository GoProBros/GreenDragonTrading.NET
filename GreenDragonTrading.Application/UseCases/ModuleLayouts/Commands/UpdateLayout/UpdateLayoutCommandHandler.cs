using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.ModuleLayouts.Commands.UpdateLayout;

/// <summary>
/// Handler for UpdateLayoutCommand
/// </summary>
public class UpdateLayoutCommandHandler : IRequestHandler<UpdateLayoutCommand, ApiResponse<ModuleLayoutDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateLayoutCommandHandler> _logger;

    public UpdateLayoutCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        ILogger<UpdateLayoutCommandHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<ModuleLayoutDto>> Handle(
        UpdateLayoutCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();
        var isAdminOrStaff = _currentUserService.IsAdminOrStaff;

        var layout = await _uow.ModuleLayouts.GetByIdAsync(request.Id, cancellationToken);
        if (layout == null)
        {
            throw new NotFoundException("Layout không tồn tại.");
        }

        // User can update only own layout.
        // Admin/Staff can update own layout and system layout (UserId == null).
        var canUpdate = layout.UserId == userId || (isAdminOrStaff && layout.UserId == null);
        if (!canUpdate)
        {
            throw new AccessDeniedException("Bạn không có quyền cập nhật layout này.");
        }

        // Update fields
        layout.LayoutName = !string.IsNullOrWhiteSpace(request.LayoutName) ? request.LayoutName.Trim() : layout.LayoutName;
        layout.ConfigJson = request.ConfigJson.HasValue ? JsonSerializer.Serialize(request.ConfigJson.Value) : layout.ConfigJson;
        layout.IsSystemDefault = request.IsSystemDefault ?? layout.IsSystemDefault;
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

        _logger.LogInformation("Layout updated successfully {LayoutId} for user {UserId}", layout.Id, userId);

        return ApiResponse<ModuleLayoutDto>.Success(result, "Cập nhật layout thành công");
    }
}
