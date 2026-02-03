using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReportCategories.Commands.DeleteCategory;

/// <summary>
/// Command to delete an analysis report category (soft delete)
/// </summary>
public class DeleteCategoryCommand : IRequest<ApiResponse>
{
    public string Id { get; set; } = null!;
}
