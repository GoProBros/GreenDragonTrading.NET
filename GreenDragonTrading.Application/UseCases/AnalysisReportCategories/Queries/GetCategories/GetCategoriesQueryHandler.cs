using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReportCategories.Queries.GetCategories;

/// <summary>
/// Handler for GetCategoriesQuery
/// </summary>
public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, ApiResponse<PaginatedResponse<AnalysisReportCategoryDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetCategoriesQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ApiResponse<PaginatedResponse<AnalysisReportCategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        // Get all categories
        var allCategories = (await _uow.AnalysisReportCategories.GetAllAsync(cancellationToken)).AsQueryable();

        // Filter by status if provided
        if (request.Status.HasValue)
        {
            allCategories = allCategories.Where(c => c.Status == request.Status.Value);
        }

        // Filter by level if provided
        if (request.Level.HasValue)
        {
            allCategories = allCategories.Where(c => c.Level == request.Level.Value);
        }

        var categoriesList = allCategories
            .OrderBy(c => c.Name)
            .ToList();

        var totalCount = categoriesList.Count;

        var pagedCategories = categoriesList
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = pagedCategories.Select(c => new AnalysisReportCategoryDto
        {
            Code = c.Code,
            Name = c.Name,
            Description = c.Description,
            Level = c.Level,
            ParentId = c.ParentId,
            Status = c.Status,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            ChildCategories = categoriesList
                .Where(ch => ch.ParentId == c.Code)
                .Select(ch => new AnalysisReportCategoryDto
                {
                    Code = ch.Code,
                    Name = ch.Name,
                    Description = ch.Description,
                    Level = ch.Level,
                    ParentId = ch.ParentId,
                    Status = ch.Status
                }).ToList()
        }).ToList();

        var paginatedResponse = PaginatedResponse<AnalysisReportCategoryDto>.Create(
            dtos,
            totalCount,
            request.PageIndex,
            request.PageSize
        );

        return ApiResponse<PaginatedResponse<AnalysisReportCategoryDto>>.Success(paginatedResponse, "Lấy danh sách danh mục thành công");
    }
}
