using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReports.Commands.UpdateReport;

/// <summary>
/// Command to update an existing analysis report
/// </summary>
public class UpdateReportCommand : IRequest<ApiResponse<AnalysisReportDto>>
{
    public Guid Id { get; set; }
    public string? SourceId { get; set; }
    public string? CategoryId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string[]? Tickers { get; set; }
    public string? SectorId { get; set; }
    public DateTimeOffset? PublishDate { get; set; }
    public CommonStatus? Status { get; set; }
}
