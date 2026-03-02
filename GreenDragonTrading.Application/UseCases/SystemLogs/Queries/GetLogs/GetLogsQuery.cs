using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.SystemLogs.Queries.GetLogs
{
    /// <summary>
    /// Query to retrieve paginated and filtered log entries.
    /// </summary>
    /// <param name="Date">Date of the log to read (yyyy-MM-dd). Defaults to today.</param>
    /// <param name="Level">Minimum log level: Verbose, Debug, Information, Warning, Error, Fatal.</param>
    /// <param name="Search">Full-text search within message, exception, and source context.</param>
    public record GetLogsQuery(
        DateOnly? Date,
        string? Level,
        string? Search) : PaginationQuery, IRequest<ApiResponse<PaginatedResponse<LogEntryDto>>>;
}
