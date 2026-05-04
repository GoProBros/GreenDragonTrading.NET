using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.SystemLogs.Queries.GetLogDates
{
    /// <summary>
    /// Query to retrieve the list of available log file dates.
    /// </summary>
    public record GetLogDatesQuery : IRequest<ApiResponse<LogDatesResult>>;
}
