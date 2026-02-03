using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReportSources.Commands.DeleteSource;

/// <summary>
/// Command to delete an analysis report source
/// </summary>
public class DeleteSourceCommand : IRequest<ApiResponse>
{
    public string Id { get; set; } = null!;
}
