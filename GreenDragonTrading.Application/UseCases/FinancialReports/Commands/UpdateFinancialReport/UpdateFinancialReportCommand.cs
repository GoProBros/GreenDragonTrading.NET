using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Commands.UpdateFinancialReport
{
    /// <summary>
    /// Command to update an existing financial report
    /// </summary>
    public record UpdateFinancialReportCommand(
        Guid Id,
        FinancialReportData? ReportData,
        FinancialReportStatus? Status
    ) : IRequest<ApiResponse<FinancialReportDto>>;
}
