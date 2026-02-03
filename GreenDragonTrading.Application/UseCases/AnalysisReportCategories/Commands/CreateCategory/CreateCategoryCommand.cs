using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReportCategories.Commands.CreateCategory;

/// <summary>
/// Command to create a new analysis report category
/// </summary>
public class CreateCategoryCommand : IRequest<ApiResponse<AnalysisReportCategoryDto>>
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int Level { get; set; }
    public string? ParentId { get; set; }
}
