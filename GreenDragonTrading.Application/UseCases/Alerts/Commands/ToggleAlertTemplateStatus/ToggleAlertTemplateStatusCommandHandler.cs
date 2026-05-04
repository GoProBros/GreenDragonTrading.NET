using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Commands.ToggleAlertTemplateStatus;

public class ToggleAlertTemplateStatusCommandHandler(
    IUnitOfWork uow,
    ICurrentUserService currentUserService)
    : IRequestHandler<ToggleAlertTemplateStatusCommand, ApiResponse<AlertTemplateDto>>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<ApiResponse<AlertTemplateDto>> Handle(
        ToggleAlertTemplateStatusCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdminOrStaff)
        {
            throw new AccessDeniedException("Chi admin/staff moi co quyen doi trang thai alert template.");
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

        var nextStatus = !template.IsActive;
        if (nextStatus && template.Type.HasValue && template.Condition.HasValue)
        {
            var conflict = await _uow.AlertTemplates.FirstOrDefaultAsync(
                x => x.IsActive
                    && x.Type == template.Type
                    && x.Condition == template.Condition
                    && x.Id != template.Id,
                cancellationToken);
            if (conflict != null)
            {
                throw new ConflictException("Da ton tai alert template dang hoat dong cho cap type/condition nay.");
            }
        }

        template.IsActive = nextStatus;
        template.UpdatedAt = DateTimeOffset.UtcNow;
        _uow.AlertTemplates.Update(template);
        await _uow.SaveChangesAsync(cancellationToken);

        var statusText = template.IsActive ? "bat" : "tat";
        return ApiResponse<AlertTemplateDto>.Success(MapToDto(template), $"Doi trang thai alert template thanh cong ({statusText}).");
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
