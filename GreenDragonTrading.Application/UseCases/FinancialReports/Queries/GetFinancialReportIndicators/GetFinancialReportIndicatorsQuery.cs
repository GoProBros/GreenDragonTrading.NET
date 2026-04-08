using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Domain.Entities;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Queries.GetFinancialReportIndicators
{
    /// <summary>
    /// Query to get calculated indicator data of a financial report by ID.
    /// </summary>
    public record GetFinancialReportIndicatorsQuery(Guid Id) : IRequest<ApiResponse<FinancialReportIndicatorData>>;
}
