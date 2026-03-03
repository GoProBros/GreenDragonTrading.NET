using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace GreenDragonTrading.Infrastructure.Services
{
    /// <summary>
    /// Reads and parses compact JSON log files written by Serilog.
    /// </summary>
    public class LogReaderService : ILogReaderService
    {
        private readonly string _logDirectory;
        private readonly string _jsonFilePrefix;
        private readonly ILogger<LogReaderService> _logger;

        // Log level severity order for "minimum level" filtering
        private static readonly Dictionary<string, int> LevelOrder = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Verbose"] = 0,
            ["Debug"] = 1,
            ["Information"] = 2,
            ["Warning"] = 3,
            ["Error"] = 4,
            ["Fatal"] = 5,
        };

        public LogReaderService(
            IHostEnvironment hostEnvironment,
            ILogger<LogReaderService> logger,
            IOptions<LogOptions> logOptions)
        {
            _logger = logger;

            var options = logOptions.Value;
            _jsonFilePrefix = options.JsonFilePrefix;

            var dir = options.Directory;
            _logDirectory = Path.IsPathRooted(dir)
                ? dir
                : Path.Combine(hostEnvironment.ContentRootPath, dir);

            _logger.LogInformation("LogReaderService initialized. Directory={LogDirectory}, Prefix={Prefix}",
                _logDirectory, _jsonFilePrefix);
        }

        /// <inheritdoc />
        public Task<List<DateOnly>> GetAvailableDatesAsync(CancellationToken cancellationToken = default)
        {
            var dates = new List<DateOnly>();

            if (!Directory.Exists(_logDirectory))
                return Task.FromResult(dates);

            // JSON log files follow pattern: {prefix}yyyyMMdd.json (possibly with _NNN suffix for size-rolled files)
            var files = Directory.GetFiles(_logDirectory, $"{_jsonFilePrefix}*.json");

            foreach (var file in files)
            {
                var dateStr = ExtractDateFromFileName(Path.GetFileNameWithoutExtension(file), _jsonFilePrefix);
                if (dateStr is not null && DateOnly.TryParseExact(dateStr, "yyyyMMdd", out var date))
                    dates.Add(date);
            }

            dates = dates.Distinct().OrderByDescending(d => d).ToList();
            return Task.FromResult(dates);
        }

        /// <inheritdoc />
        public async Task<(List<LogEntryDto> Entries, int TotalCount)> GetLogsAsync(
            DateOnly? date,
            string? level,
            string? search,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var targetDate = date ?? DateOnly.FromDateTime(DateTime.Now);
            var minSeverity = level is not null && LevelOrder.TryGetValue(level, out var sv) ? sv : LevelOrder["Information"];

            var entries = await ReadEntriesFromDateAsync(targetDate, cancellationToken);

            // Filter by level
            entries = entries
                .Where(e => LevelOrder.TryGetValue(e.Level, out var s) && s >= minSeverity)
                .ToList();

            // Filter by search text
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                entries = entries
                    .Where(e =>
                        e.Message.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        (e.Exception?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (e.SourceContext?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();
            }

            var totalCount = entries.Count;
            var pagedEntries = entries
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (pagedEntries, totalCount);
        }

        private async Task<List<LogEntryDto>> ReadEntriesFromDateAsync(DateOnly date, CancellationToken cancellationToken)
        {
            var entries = new List<LogEntryDto>();

            if (!Directory.Exists(_logDirectory))
                return entries;

            var datePattern = date.ToString("yyyyMMdd");
            // Collect all files for the date (including size-rolled e.g. log-json-20260302_001.json)
            var files = Directory.GetFiles(_logDirectory, $"{_jsonFilePrefix}{datePattern}*.json")
                                 .OrderBy(f => f)
                                 .ToArray();

            foreach (var file in files)
            {
                try
                {
                    entries.AddRange(await ParseLogFileAsync(file, cancellationToken));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse log file {File}", file);
                }
            }

            // Most recent first
            entries.Sort((a, b) => b.Timestamp.CompareTo(a.Timestamp));
            return entries;
        }

        private static async Task<List<LogEntryDto>> ParseLogFileAsync(string filePath, CancellationToken cancellationToken)
        {
            var entries = new List<LogEntryDto>();

            // Open with shared read access so Serilog can still write to the file
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var reader = new StreamReader(fs);

            string? line;
            while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    var entry = ParseLine(line);
                    if (entry is not null)
                        entries.Add(entry);
                }
                catch
                {
                    // Skip malformed lines
                }
            }

            return entries;
        }

        private static LogEntryDto? ParseLine(string line)
        {
            using var doc = JsonDocument.Parse(line);
            var root = doc.RootElement;

            // @t = timestamp
            if (!root.TryGetProperty("@t", out var tProp)) return null;
            if (!DateTimeOffset.TryParse(tProp.GetString(), out var timestamp)) return null;

            // @l = level (omitted means Information in CompactJsonFormatter)
            var level = root.TryGetProperty("@l", out var lProp) ? (lProp.GetString() ?? "Information") : "Information";

            // @m = rendered message; fallback to @mt
            string message;
            if (root.TryGetProperty("@m", out var mProp) && mProp.GetString() is { } m)
                message = m;
            else if (root.TryGetProperty("@mt", out var mtProp) && mtProp.GetString() is { } mt)
                message = mt;
            else
                message = string.Empty;

            // @x = exception
            var exception = root.TryGetProperty("@x", out var xProp) ? xProp.GetString() : null;

            // Well-known enriched properties
            var sourceContext = root.TryGetProperty("SourceContext", out var scProp) ? scProp.GetString() : null;
            var requestPath = root.TryGetProperty("RequestPath", out var rpProp) ? rpProp.GetString() : null;
            var machineName = root.TryGetProperty("MachineName", out var mnProp) ? mnProp.GetString() : null;
            int? threadId = root.TryGetProperty("ThreadId", out var tidProp) && tidProp.ValueKind == JsonValueKind.Number
                ? tidProp.GetInt32()
                : null;

            // Collect remaining properties (excluding Serilog reserved)
            var reserved = new HashSet<string> { "@t", "@l", "@m", "@mt", "@mt", "@x", "@i", "@r",
                "SourceContext", "RequestPath", "MachineName", "ThreadId", "EnvironmentName" };

            var props = new Dictionary<string, object?>();
            foreach (var prop in root.EnumerateObject())
            {
                if (reserved.Contains(prop.Name)) continue;
                props[prop.Name] = prop.Value.ValueKind switch
                {
                    JsonValueKind.String => prop.Value.GetString(),
                    JsonValueKind.Number => prop.Value.TryGetInt64(out var l) ? (object?)l : prop.Value.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    _ => prop.Value.GetRawText(),
                };
            }

            return new LogEntryDto
            {
                Timestamp = timestamp,
                Level = level,
                Message = message,
                Exception = exception,
                SourceContext = sourceContext,
                RequestPath = requestPath,
                MachineName = machineName,
                ThreadId = threadId,
                Properties = props,
            };
        }

        /// <summary>
        /// Extracts the 8-digit date string from a file name like "log-json-20260302" or "log-json-20260302_001".
        /// </summary>
        private static string? ExtractDateFromFileName(string fileNameWithoutExt, string prefix)
        {
            if (!fileNameWithoutExt.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return null;
            var rest = fileNameWithoutExt[prefix.Length..];
            // Take first 8 chars (yyyyMMdd)
            return rest.Length >= 8 ? rest[..8] : null;
        }
    }
}
