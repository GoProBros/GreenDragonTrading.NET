using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReportCategories.Commands.UpdateCategory;

/// <summary>
/// Handler for UpdateCategoryCommand
/// </summary>
public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, ApiResponse<AnalysisReportCategoryDto>>
{
    private readonly IUnitOfWork _uow;

    public UpdateCategoryCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ApiResponse<AnalysisReportCategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _uow.AnalysisReportCategories.GetByIdAsync(request.Id, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException($"Danh mục với ID '{request.Id}' không tồn tại");
        }

        // Validate parent if provided
        if (request.ParentId != null)
        {
            if (!string.IsNullOrEmpty(request.ParentId))
            {
                var parent = await _uow.AnalysisReportCategories.GetByIdAsync(request.ParentId, cancellationToken);
                if (parent == null)
                {
                    throw new NotFoundException($"Parent category '{request.ParentId}' không tồn tại");
                }
            }
            category.ParentId = request.ParentId;
        }

        // Update fields
        if (!string.IsNullOrEmpty(request.Name))
        {
            category.Name = request.Name;
        }

        if (request.Description != null)
        {
            category.Description = request.Description;
        }

        if (request.Level.HasValue)
        {
            category.Level = request.Level.Value;
        }

        if (request.Status.HasValue)
        {
            category.Status = request.Status.Value;
        }

        category.UpdatedAt = DateTimeOffset.UtcNow;

        _uow.AnalysisReportCategories.Update(category);
        await _uow.SaveChangesAsync(cancellationToken);

        var dto = new AnalysisReportCategoryDto
        {
            Code = category.Code,
            Name = category.Name,
            Description = category.Description,
            Level = category.Level,
            ParentId = category.ParentId,
            Status = category.Status,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };

        return ApiResponse<AnalysisReportCategoryDto>.Success(dto, "Cập nhật danh mục thành công");
    }
}
