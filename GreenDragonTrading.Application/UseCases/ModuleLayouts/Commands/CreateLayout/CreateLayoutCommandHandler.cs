using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.ModuleLayouts.Commands.CreateLayout;

public class CreateLayoutCommandHandler : IRequestHandler<CreateLayoutCommand, ApiResponse<ModuleLayoutDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateLayoutCommandHandler> _logger;

    public CreateLayoutCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        ILogger<CreateLayoutCommandHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<ModuleLayoutDto>> Handle(
        CreateLayoutCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();

        var configJsonString = JsonSerializer.Serialize(request.ConfigJson);

        var layout = new ModuleLayout
        {
            LayoutName = request.LayoutName.Trim(),
            ModuleType = request.ModuleType,
            ConfigJson = configJsonString,
            IsSystemDefault = request.IsSystemDefault,
            UserId = request.IsSystemDefault ? null : userId,
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

        _logger.LogInformation("Layout created successfully {LayoutId} for user {UserId}", layout.Id, userId);

        return ApiResponse<ModuleLayoutDto>.Success(result, "Tạo layout mới thành công");
    }
}
