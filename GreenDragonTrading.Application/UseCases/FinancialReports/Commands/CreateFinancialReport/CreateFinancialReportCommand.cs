using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Commands.CreateFinancialReport
{
    /// <summary>
    /// Command to create a new financial report
    /// </summary>
    public record CreateFinancialReportCommand(
        string Ticker,
        int Year,
        ReportPeriod Period,
        FinancialReportData ReportData
    ) : IRequest<ApiResponse<FinancialReportDto>>;
}
