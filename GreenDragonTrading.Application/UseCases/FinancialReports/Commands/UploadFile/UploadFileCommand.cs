using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Commands.UploadFile
{
    /// <summary>
    /// Command to upload or replace file for financial report
    /// </summary>
    public record UploadFileCommand(
        Guid Id,
        IFormFile File
    ) : IRequest<ApiResponse<FinancialReportDto>>;
}
