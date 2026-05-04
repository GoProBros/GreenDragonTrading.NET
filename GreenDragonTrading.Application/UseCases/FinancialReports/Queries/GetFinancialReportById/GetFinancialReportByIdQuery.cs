using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Queries.GetFinancialReportById
{
    /// <summary>
    /// Query to get financial report by ID
    /// </summary>
    public record GetFinancialReportByIdQuery(Guid Id) : IRequest<ApiResponse<FinancialReportDto>>;
}
