using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.SystemLogs.Queries.GetLogDates;
using GreenDragonTrading.Application.UseCases.SystemLogs.Queries.GetLogs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for reading application log files.
    /// </summary>
    [Route("api/v1/logs")]
    [ApiController]
    [Authorize]
    public class LogController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LogController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Retrieves paginated log entries, optionally filtered by date, level, and search text.
        /// </summary>
        /// <param name="date">Date in yyyy-MM-dd format. Defaults to today.</param>
        /// <param name="level">Minimum level: Verbose, Debug, Information, Warning, Error, Fatal. Defaults to Information.</param>
        /// <param name="search">Full-text search within message, exception, and source context.</param>
        /// <param name="pageIndex">1-based page index. Defaults to 1.</param>
        /// <param name="pageSize">Entries per page (1–500). Defaults to 10.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated log entries matching the filter criteria.</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<LogEntryDto>>>> GetLogs(
            [FromQuery] DateOnly? date,
            [FromQuery] string? level,
            [FromQuery] string? search,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(
                new GetLogsQuery(date, level, search)
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                },
                cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Retrieves the list of dates for which log files are available.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Descending list of available log dates.</returns>
        [HttpGet("dates")]
        public async Task<ActionResult<ApiResponse<LogDatesResult>>> GetAvailableDates(
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetLogDatesQuery(), cancellationToken);
            return Ok(result);
        }
    }
}
