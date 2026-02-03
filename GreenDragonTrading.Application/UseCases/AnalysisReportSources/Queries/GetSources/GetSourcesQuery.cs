using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReportSources.Queries.GetSources;

/// <summary>
/// Query to get all analysis report sources
/// </summary>
public record GetSourcesQuery : PaginationQuery, IRequest<ApiResponse<PaginatedResponse<AnalysisReportSourceDto>>>
{
    public CommonStatus? Status { get; set; }
}
