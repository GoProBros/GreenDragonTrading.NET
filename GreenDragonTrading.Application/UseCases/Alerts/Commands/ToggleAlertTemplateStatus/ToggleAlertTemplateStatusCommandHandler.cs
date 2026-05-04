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
            throw new AccessDeniedException("Chỉ admin/staff mới có quyền đổi trạng thái alert template.");
        }

        if (request.Id <= 0)
        {
            throw new BusinessRuleException("Id template không hợp lệ.");
        }

        var template = await _uow.AlertTemplates.GetByIdAsync(request.Id, cancellationToken);
        if (template == null)
        {
            throw new NotFoundException("Không tìm thấy alert template.");
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
                throw new ConflictException("Đã tồn tại alert template đang hoạt động cho cặp type/condition này.");
            }
        }

        template.IsActive = nextStatus;
        template.UpdatedAt = DateTimeOffset.UtcNow;
        _uow.AlertTemplates.Update(template);
        await _uow.SaveChangesAsync(cancellationToken);

        var statusText = template.IsActive ? "bật" : "tắt";
        return ApiResponse<AlertTemplateDto>.Success(MapToDto(template), $"Đổi trạng thái alert template thành công ({statusText}).");
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
