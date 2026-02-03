using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReports.Queries.GetReportById;

/// <summary>
/// Query to get a single analysis report by ID
/// </summary>
public record GetReportByIdQuery : IRequest<ApiResponse<AnalysisReportDto>>
{
    public Guid Id { get; init; }
}
