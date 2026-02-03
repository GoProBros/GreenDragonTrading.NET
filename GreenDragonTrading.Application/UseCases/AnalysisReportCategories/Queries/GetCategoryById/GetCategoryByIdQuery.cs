using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReportCategories.Queries.GetCategoryById;

/// <summary>
/// Query to get a single analysis report category by ID
/// </summary>
public record GetCategoryByIdQuery : IRequest<ApiResponse<AnalysisReportCategoryDto>>
{
    public string Id { get; init; } = null!;
}
