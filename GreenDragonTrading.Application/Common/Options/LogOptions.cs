namespace GreenDragonTrading.Application.Common.Options
{
    public class LogOptions
    {
        public const string SectionName = "LogOptions";

        /// <summary>
        /// Directory where Serilog writes log files.
        /// Relative paths are resolved from the application's ContentRootPath.
        /// </summary>
        public string Directory { get; set; } = "Logs";

        /// <summary>
        /// Filename prefix for compact JSON log files (used by LogReaderService).
        /// Default matches the Serilog sink path pattern.
        /// </summary>
        public string JsonFilePrefix { get; set; } = "log-json-";
    }
}
