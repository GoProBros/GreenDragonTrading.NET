using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Queries.GetAlertTemplates;

public class GetAlertTemplatesQueryHandler(
    IUnitOfWork uow,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetAlertTemplatesQuery, ApiResponse<PaginatedResponse<AlertTemplateDto>>>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<ApiResponse<PaginatedResponse<AlertTemplateDto>>> Handle(
        GetAlertTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdminOrStaff)
        {
            throw new AccessDeniedException("Chỉ admin/staff mới có quyền xem alert template.");
        }

        var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var templates = await _uow.AlertTemplates.GetAllAsync(cancellationToken);

        var filtered = templates
            .Where(x => request.Type == null || x.Type == request.Type)
            .Where(x => request.Condition == null || x.Condition == request.Condition)
            .Where(x => request.IsActive == null || x.IsActive == request.IsActive)
            .Where(x => request.IsDefault == null || x.IsDefault == request.IsDefault)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .ToList();

        var totalCount = filtered.Count;
        var items = filtered
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto)
            .ToList();

        var paginatedResponse = PaginatedResponse<AlertTemplateDto>.Create(
            items,
            totalCount,
            pageIndex,
            pageSize);

        return ApiResponse<PaginatedResponse<AlertTemplateDto>>.Success(
            paginatedResponse,
            "Lấy danh sách alert template thành công.");
    }

    private static AlertTemplateDto MapToDto(Domain.Entities.AlertTemplate template)
    {
        return new AlertTemplateDto
        {
            Id = template.Id,
            Type = template.Type,
            Condition = template.Condition,
            TitleTemplate = template.TitleTemplate,
            BodyTemplate = template.BodyTemplate,
            IsActive = template.IsActive,
            IsDefault = template.IsDefault,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };
    }
}
