using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Queries.GetAlertTemplateById;

public class GetAlertTemplateByIdQueryHandler(
    IUnitOfWork uow,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetAlertTemplateByIdQuery, ApiResponse<AlertTemplateDto>>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<ApiResponse<AlertTemplateDto>> Handle(
        GetAlertTemplateByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdminOrStaff)
        {
            throw new AccessDeniedException("Chi admin/staff moi co quyen xem alert template.");
        }

        if (request.Id <= 0)
        {
            throw new BusinessRuleException("Id template khong hop le.");
        }

        var template = await _uow.AlertTemplates.GetByIdAsync(request.Id, cancellationToken);
        if (template == null)
        {
            throw new NotFoundException("Khong tim thay alert template.");
        }

        return ApiResponse<AlertTemplateDto>.Success(MapToDto(template), "Lay alert template thanh cong.");
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
