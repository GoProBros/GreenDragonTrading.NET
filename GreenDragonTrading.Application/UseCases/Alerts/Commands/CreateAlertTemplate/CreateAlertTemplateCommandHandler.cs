using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Commands.CreateAlertTemplate;

public class CreateAlertTemplateCommandHandler(
    IUnitOfWork uow,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateAlertTemplateCommand, ApiResponse<AlertTemplateDto>>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<ApiResponse<AlertTemplateDto>> Handle(
        CreateAlertTemplateCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdminOrStaff)
        {
            throw new AccessDeniedException("Chỉ admin/staff mới có quyền tạo alert template.");
        }

        var now = DateTimeOffset.UtcNow;
        var template = new AlertTemplate
        {
            Type = request.Type,
            Condition = request.Condition,
            TitleTemplate = request.TitleTemplate.Trim(),
            BodyTemplate = request.BodyTemplate.Trim(),
            IsActive = request.IsActive,
            IsDefault = request.IsDefault,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _uow.AlertTemplates.AddAsync(template, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return ApiResponse<AlertTemplateDto>.Success(MapToDto(template), "Tạo alert template thành công.");
    }

    private static AlertTemplateDto MapToDto(AlertTemplate template)
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
