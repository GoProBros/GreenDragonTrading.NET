using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReports.Queries.GetReports;

/// <summary>
/// Query to get analysis reports with filters and pagination
/// </summary>
public record GetReportsQuery : PaginationQuery, IRequest<ApiResponse<PaginatedResponse<AnalysisReportDto>>>
{
    public string? SourceId { get; init; }
    public string? CategoryId { get; init; }
    public string? Ticker { get; init; }
    public string? SectorId { get; init; }
    public string? SearchTerm { get; init; }
    public CommonStatus? Status { get; init; }
}
