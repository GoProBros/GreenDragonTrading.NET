using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReports.Commands.CreateReport;

/// <summary>
/// Command to create a new analysis report (without file)
/// </summary>
public class CreateReportCommand : IRequest<ApiResponse<AnalysisReportDto>>
{
    public string SourceId { get; set; } = null!;
    public string CategoryId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string[]? Tickers { get; set; }
    public string? SectorId { get; set; }
    public DateTimeOffset? PublishDate { get; set; }
    public string? Author { get; set; }
}
