using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace GreenDragonTrading.Application.UseCases.AnalysisReports.Commands.UploadReport;

/// <summary>
/// Command to upload a new analysis report with file
/// </summary>
public class UploadReportCommand : IRequest<ApiResponse<AnalysisReportDto>>
{
    public IFormFile File { get; set; } = null!;
    public UploadAnalysisReportDto Metadata { get; set; } = null!;
}
