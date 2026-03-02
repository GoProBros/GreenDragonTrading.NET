namespace GreenDragonTrading.Application.DTOs
{
    /// <summary>
    /// Represents a single log entry parsed from a Serilog compact JSON log file.
    /// </summary>
    public record LogEntryDto
    {
        /// <summary>UTC timestamp of the log entry.</summary>
        public DateTimeOffset Timestamp { get; init; }

        /// <summary>Log level: Verbose, Debug, Information, Warning, Error, Fatal.</summary>
        public string Level { get; init; } = null!;

        /// <summary>Rendered log message.</summary>
        public string Message { get; init; } = null!;

        /// <summary>Exception details, if any.</summary>
        public string? Exception { get; init; }

        /// <summary>Source context (logger name / class name).</summary>
        public string? SourceContext { get; init; }

        /// <summary>HTTP request path, if available.</summary>
        public string? RequestPath { get; init; }

        /// <summary>Thread ID.</summary>
        public int? ThreadId { get; init; }

        /// <summary>Machine name.</summary>
        public string? MachineName { get; init; }

        /// <summary>Additional structured properties captured with the entry.</summary>
        public Dictionary<string, object?> Properties { get; init; } = new();
    }

    /// <summary>
    /// Available log dates result.
    /// </summary>
    public record LogDatesResult
    {
        /// <summary>Available log dates, descending, derived from existing daily JSON log files.</summary>
        public List<DateOnly> Dates { get; init; } = new();
    }
}
