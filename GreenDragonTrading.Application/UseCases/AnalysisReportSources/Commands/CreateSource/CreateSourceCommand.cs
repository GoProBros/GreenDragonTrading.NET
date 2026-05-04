using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReportSources.Commands.CreateSource;

/// <summary>
/// Command to create a new analysis report source
/// </summary>
public class CreateSourceCommand : IRequest<ApiResponse<AnalysisReportSourceDto>>
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
}
