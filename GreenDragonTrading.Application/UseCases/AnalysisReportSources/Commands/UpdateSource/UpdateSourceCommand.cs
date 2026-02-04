using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReportSources.Commands.UpdateSource;

/// <summary>
/// Command to update an existing analysis report source
/// </summary>
public class UpdateSourceCommand : IRequest<ApiResponse<AnalysisReportSourceDto>>
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public CommonStatus Status { get; set; }
}
