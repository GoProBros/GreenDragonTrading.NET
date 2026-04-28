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
    : IRequestHandler<GetAlertTemplatesQuery, ApiResponse<List<AlertTemplateDto>>>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<ApiResponse<List<AlertTemplateDto>>> Handle(
        GetAlertTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdminOrStaff)
        {
            throw new AccessDeniedException("Chi admin/staff moi co quyen xem alert template.");
        }

        var templates = await _uow.AlertTemplates.GetAllAsync(cancellationToken);

        var filtered = templates
            .Where(x => request.Type == null || x.Type == request.Type)
            .Where(x => request.Condition == null || x.Condition == request.Condition)
            .Where(x => request.IsActive == null || x.IsActive == request.IsActive)
            .Where(x => request.IsDefault == null || x.IsDefault == request.IsDefault)
            .Select(MapToDto)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .ToList();

        return ApiResponse<List<AlertTemplateDto>>.Success(filtered, "Lay danh sach alert template thanh cong.");
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
