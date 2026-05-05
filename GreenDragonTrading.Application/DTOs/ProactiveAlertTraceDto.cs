using System.Text.Json.Serialization;

namespace GreenDragonTrading.Application.DTOs
{
    /// <summary>
    /// A single step within a proactive alert trace, capturing the outcome of one stage in the pipeline.
    /// </summary>
    public class ProactiveAlertTraceStepDto
    {
        /// <summary>
        /// Pipeline stage identifier.
        /// Values: trigger, layerA_filter, layerB_filter, cooldown_filter, enqueued, dequeued, ai_evaluation, broadcast
        /// </summary>
        public string Step { get; set; } = string.Empty;

        /// <summary>
        /// Outcome of this step: pass, fail, success, skipped, error.
        /// </summary>
        public string Result { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable reason when Result is fail or error.
        /// </summary>
        public string? FailReason { get; set; }

        /// <summary>
        /// Arbitrary key-value payload capturing the data evaluated at this step.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, object?>? Detail { get; set; }

        /// <summary>
        /// UTC timestamp when this step was recorded.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Full lifecycle trace of one proactive alert signal from trigger to broadcast.
    /// Stored in Redis with a 7-day TTL for evidence and demo purposes.
    /// </summary>
    public class ProactiveAlertTraceDto
    {
        /// <summary>
        /// Unique identifier for this trace (UUID).
        /// </summary>
        public string TraceId { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Stock ticker (uppercase).
        /// </summary>
        public string Ticker { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when the trace was first created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// UTC timestamp of the last update to this trace.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ordered list of pipeline steps recorded so far.
        /// </summary>
        public List<ProactiveAlertTraceStepDto> Steps { get; set; } = new();

        /// <summary>
        /// High-level summary of the final outcome.
        /// Values: broadcast_success, ai_fallback, ai_failed, filter_failed_layerA, filter_failed_layerB,
        ///         filter_failed_cooldown, filter_failed_no_users, dequeued_no_active_users, error
        /// </summary>
        public string FinalResult { get; set; } = "pending";

        /// <summary>
        /// Number of users that were candidates for this signal.
        /// </summary>
        public int CandidateUserCount { get; set; }

        /// <summary>
        /// Number of users that passed the cooldown filter.
        /// </summary>
        public int EligibleUserCount { get; set; }

        /// <summary>
        /// Number of users that were actually notified (after active-user check + broadcast).
        /// </summary>
        public int NotifiedUserCount { get; set; }
    }

    /// <summary>
    /// Lightweight summary row used in list endpoints (excludes full step detail payload).
    /// </summary>
    public class ProactiveAlertTraceSummaryDto
    {
        public string TraceId { get; set; } = string.Empty;
        public string Ticker { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string FinalResult { get; set; } = "pending";
        public int StepCount { get; set; }
        public int CandidateUserCount { get; set; }
        public int EligibleUserCount { get; set; }
        public int NotifiedUserCount { get; set; }
    }

    /// <summary>
    /// Aggregated statistics for the evidence dashboard.
    /// </summary>
    public class ProactiveAlertEvidenceStatsDto
    {
        /// <summary>
        /// Total number of traces recorded.
        /// </summary>
        public int TotalTraces { get; set; }

        /// <summary>
        /// Traces grouped by FinalResult.
        /// </summary>
        public Dictionary<string, int> ByFinalResult { get; set; } = new();

        /// <summary>
        /// Count of traces where AI evaluation was attempted (success or fallback).
        /// </summary>
        public int AiEvaluationAttempted { get; set; }

        /// <summary>
        /// Count of traces where AI evaluation succeeded (no fallback).
        /// </summary>
        public int AiEvaluationSuccess { get; set; }

        /// <summary>
        /// Count of traces where deterministic fallback was used.
        /// </summary>
        public int AiEvaluationFallback { get; set; }

        /// <summary>
        /// Count of traces where AI call failed entirely.
        /// </summary>
        public int AiEvaluationFailed { get; set; }

        /// <summary>
        /// Average AI evaluation latency in milliseconds (successful calls only).
        /// </summary>
        public double AvgAiLatencyMs { get; set; }

        /// <summary>
        /// Total number of users notified across all broadcast_success traces.
        /// </summary>
        public int TotalNotifiedUsers { get; set; }

        /// <summary>
        /// Distribution of traces by ticker (top 20).
        /// </summary>
        public Dictionary<string, int> TopTickers { get; set; } = new();

        /// <summary>
        /// Distribution of traces by hour of day (UTC).
        /// </summary>
        public Dictionary<int, int> ByHour { get; set; } = new();

        /// <summary>
        /// Time range covered by the statistics.
        /// </summary>
        public DateTime? OldestTrace { get; set; }
        public DateTime? NewestTrace { get; set; }
    }

    /// <summary>
    /// Request model for listing traces with optional filters.
    /// </summary>
    public class ProactiveAlertTraceQueryDto
    {
        /// <summary>
        /// Filter by ticker (case-insensitive partial match). Leave empty for all tickers.
        /// </summary>
        public string? Ticker { get; set; }

        /// <summary>
        /// Filter by FinalResult value.
        /// </summary>
        public string? FinalResult { get; set; }

        /// <summary>
        /// Only return traces created after this UTC datetime.
        /// </summary>
        public DateTime? From { get; set; }

        /// <summary>
        /// Only return traces created before this UTC datetime.
        /// </summary>
        public DateTime? To { get; set; }

        /// <summary>
        /// 0-based page index. Default 0.
        /// </summary>
        public int PageIndex { get; set; } = 0;

        /// <summary>
        /// Page size. Default 50, max 200.
        /// </summary>
        public int PageSize { get; set; } = 50;
    }
}
