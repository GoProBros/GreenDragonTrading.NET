using GreenDragonTrading.Application.DTOs;

namespace GreenDragonTrading.Application.Interfaces
{
    /// <summary>
    /// Service for reading and parsing structured JSON log files produced by Serilog.
    /// </summary>
    public interface ILogReaderService
    {
        /// <summary>
        /// Returns all available log dates based on existing JSON log files.
        /// </summary>
        Task<List<DateOnly>> GetAvailableDatesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Reads, filters, and paginates log entries from the JSON log file of the given date.
        /// </summary>
        /// <param name="date">Date to read. Null defaults to today (server local time).</param>
        /// <param name="level">Minimum log level (e.g. "Warning", "Error"). Null defaults to "Information".</param>
        /// <param name="search">Full-text search within message, exception, and source context.</param>
        /// <param name="pageIndex">1-based page index.</param>
        /// <param name="pageSize">Number of entries per page.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Tuple of paged entries and total matching count before paging.</returns>
        Task<(List<LogEntryDto> Entries, int TotalCount)> GetLogsAsync(
            DateOnly? date,
            string? level,
            string? search,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
