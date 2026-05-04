using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReportCategories.Queries.GetCategoryById;

/// <summary>
/// Handler for GetCategoryByIdQuery
/// </summary>
public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, ApiResponse<AnalysisReportCategoryDto>>
{
    private readonly IUnitOfWork _uow;

    public GetCategoryByIdQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ApiResponse<AnalysisReportCategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _uow.AnalysisReportCategories.GetByIdAsync(request.Id, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException($"Danh mục với ID '{request.Id}' không tồn tại");
        }

        // Get all categories to build child list
        var allCategories = (await _uow.AnalysisReportCategories.GetAllAsync(cancellationToken)).ToList();

        var dto = new AnalysisReportCategoryDto
        {
            Code = category.Code,
            Name = category.Name,
            Description = category.Description,
            Level = category.Level,
            ParentId = category.ParentId,
            Status = category.Status,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
            ChildCategories = allCategories
                .Where(c => c.ParentId == category.Code)
                .Select(c => new AnalysisReportCategoryDto
                {
                    Code = c.Code,
                    Name = c.Name,
                    Description = c.Description,
                    Level = c.Level,
                    ParentId = c.ParentId,
                    Status = c.Status
                }).ToList()
        };

        return ApiResponse<AnalysisReportCategoryDto>.Success(dto, "Lấy thông tin danh mục thành công");
    }
}
