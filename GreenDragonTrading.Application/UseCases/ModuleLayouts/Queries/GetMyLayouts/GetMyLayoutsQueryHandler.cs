using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.ModuleLayouts.Queries.GetMyLayouts;

/// <summary>
/// Handler for GetMyLayoutsQuery
/// </summary>
public class GetMyLayoutsQueryHandler : IRequestHandler<GetMyLayoutsQuery, ApiResponse<List<ModuleLayoutListItemDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetMyLayoutsQueryHandler> _logger;

    public GetMyLayoutsQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        ILogger<GetMyLayoutsQueryHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<List<ModuleLayoutListItemDto>>> Handle(
        GetMyLayoutsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();

        // Get layouts (system + personal of user)
        var layouts = await _uow.ModuleLayouts.GetByModuleTypeAsync(
            request.ModuleType,
            userId,
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
            result.Count, request.ModuleType, userId);

        return ApiResponse<List<ModuleLayoutListItemDto>>.Success(
            result,
            "Lấy danh sách layout thành công");
    }
}
