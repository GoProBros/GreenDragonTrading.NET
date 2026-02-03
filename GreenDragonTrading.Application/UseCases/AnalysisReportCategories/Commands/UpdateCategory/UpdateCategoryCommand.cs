using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReportCategories.Commands.UpdateCategory;

/// <summary>
/// Command to update an existing analysis report category
/// </summary>
public class UpdateCategoryCommand : IRequest<ApiResponse<AnalysisReportCategoryDto>>
{
    public string Id { get; set; } = null!;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? Level { get; set; }
    public string? ParentId { get; set; }
    public CommonStatus? Status { get; set; }
}
