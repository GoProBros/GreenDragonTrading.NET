using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReports.Commands.DeleteReport;

/// <summary>
/// Command to delete an analysis report (soft delete)
/// </summary>
public class DeleteReportCommand : IRequest<ApiResponse>
{
    public Guid Id { get; set; }
}
