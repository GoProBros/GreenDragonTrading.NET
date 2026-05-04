using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Commands.UpdateAlertTemplate;

public class UpdateAlertTemplateCommandHandler(
    IUnitOfWork uow,
    ICurrentUserService currentUserService)
    : IRequestHandler<UpdateAlertTemplateCommand, ApiResponse<AlertTemplateDto>>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<ApiResponse<AlertTemplateDto>> Handle(
        UpdateAlertTemplateCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdminOrStaff)
        {
            throw new AccessDeniedException("Chỉ admin/staff mới có quyền cập nhật alert template.");
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

        if (request.IsDefault)
        {
            var otherDefault = await _uow.AlertTemplates.FirstOrDefaultAsync(
                x => x.IsDefault && x.Id != request.Id,
                cancellationToken);
            if (otherDefault != null)
            {
                throw new ConflictException("Đã tồn tại alert template mặc định.");
            }
        }

        if (request.IsActive && request.Type.HasValue && request.Condition.HasValue)
        {
            var conflict = await _uow.AlertTemplates.FirstOrDefaultAsync(
                x => x.IsActive
                    && x.Type == request.Type
                    && x.Condition == request.Condition
                    && x.Id != request.Id,
                cancellationToken);
            if (conflict != null)
            {
                throw new ConflictException("Đã tồn tại alert template đang hoạt động cho cặp type/condition này.");
            }
        }

        template.Type = request.Type;
        template.Condition = request.Condition;
        template.TitleTemplate = request.TitleTemplate.Trim();
        template.BodyTemplate = request.BodyTemplate.Trim();
        template.IsActive = request.IsActive;
        template.IsDefault = request.IsDefault;
        template.UpdatedAt = DateTimeOffset.UtcNow;

        _uow.AlertTemplates.Update(template);
        await _uow.SaveChangesAsync(cancellationToken);

        return ApiResponse<AlertTemplateDto>.Success(MapToDto(template), "Cập nhật alert template thành công.");
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
