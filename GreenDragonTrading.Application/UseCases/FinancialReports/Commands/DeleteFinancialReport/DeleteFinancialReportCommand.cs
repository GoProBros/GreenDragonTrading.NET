using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Commands.DeleteFinancialReport
{
    /// <summary>
    /// Command to delete a financial report
    /// </summary>
    public record DeleteFinancialReportCommand(Guid Id) : IRequest<ApiResponse>;
}
