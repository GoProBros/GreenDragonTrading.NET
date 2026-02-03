using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReportCategories.Queries.GetCategories;

/// <summary>
/// Query to get analysis report categories with filters and pagination
/// </summary>
public record GetCategoriesQuery : PaginationQuery, IRequest<ApiResponse<PaginatedResponse<AnalysisReportCategoryDto>>>
{
    public CommonStatus? Status { get; set; }
    public int? Level { get; set; }
}
