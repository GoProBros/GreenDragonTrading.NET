using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReportSources.Queries.GetSourceById;

/// <summary>
/// Query to get an analysis report source by ID
/// </summary>
public class GetSourceByIdQuery : IRequest<ApiResponse<AnalysisReportSourceDto>>
{
    public string Id { get; set; } = null!;
}
