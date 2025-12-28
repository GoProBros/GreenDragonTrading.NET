using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Commands.UploadFinancialReport
{
    /// <summary>
    /// Command to upload financial report
    /// </summary>
    public record UploadFinancialReportCommand(
        string Ticker,
        int Year,
        ReportPeriod Period,
        IFormFile File
    ) : IRequest<ApiResponse<UploadFinancialReportResponse>>;

    /// <summary>
    /// Response DTO after successful upload
    /// </summary>
    public record UploadFinancialReportResponse
    {
        public Guid ReportId { get; init; }
        public string Message { get; init; } = null!;
        public FinancialReportDto? Report { get; init; }
    }
}
