using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.ModuleLayouts.Queries.GetLayoutById;

/// <summary>
/// Handler for GetLayoutByIdQuery
/// </summary>
public class GetLayoutByIdQueryHandler : IRequestHandler<GetLayoutByIdQuery, ApiResponse<ModuleLayoutDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetLayoutByIdQueryHandler> _logger;

    public GetLayoutByIdQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        ILogger<GetLayoutByIdQueryHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<ModuleLayoutDto>> Handle(
        GetLayoutByIdQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();
        var isAdminOrStaff = _currentUserService.IsAdminOrStaff;

        var layout = await _uow.ModuleLayouts.GetByIdAndUserIdAsync(
            request.Id,
            userId,
            includeSystemDefaults: isAdminOrStaff,
            cancellationToken);
        if (layout == null)
        {
            throw new NotFoundException("Layout không tồn tại hoặc bạn không có quyền truy cập.");
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

        _logger.LogInformation("Layout retrieved successfully {LayoutId} for user {UserId}", request.Id, userId);

        return ApiResponse<ModuleLayoutDto>.Success(result, "Lấy layout thành công");
    }
}
