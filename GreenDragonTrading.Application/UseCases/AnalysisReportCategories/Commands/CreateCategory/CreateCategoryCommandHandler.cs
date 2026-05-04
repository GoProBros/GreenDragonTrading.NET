using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReportCategories.Commands.CreateCategory;

/// <summary>
/// Handler for CreateCategoryCommand
/// </summary>
public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, ApiResponse<AnalysisReportCategoryDto>>
{
    private readonly IUnitOfWork _uow;

    public CreateCategoryCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ApiResponse<AnalysisReportCategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        // Check if category Code already exists
        var existing = await _uow.AnalysisReportCategories.GetByIdAsync(request.Code, cancellationToken);
        if (existing != null)
        {
            throw new ConflictException($"Category với Code '{request.Code}' đã tồn tại");
        }

        if (!string.IsNullOrEmpty(request.ParentId))
        {
            var parent = await _uow.AnalysisReportCategories.GetByIdAsync(request.ParentId, cancellationToken);
            if (parent == null)
            {
                throw new NotFoundException($"Parent category '{request.ParentId}' không tồn tại");
            }
        }

        var category = new AnalysisReportCategory
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            Level = request.Level,
            ParentId = request.ParentId,
            Status = CommonStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _uow.AnalysisReportCategories.AddAsync(category, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        var dto = new AnalysisReportCategoryDto
        {
            Code = category.Code,
            Name = category.Name,
            Description = category.Description,
            Level = category.Level,
            ParentId = category.ParentId,
            Status = category.Status,
            CreatedAt = category.CreatedAt
        };

        return ApiResponse<AnalysisReportCategoryDto>.Success(dto, "Tạo danh mục thành công");
    }
}
