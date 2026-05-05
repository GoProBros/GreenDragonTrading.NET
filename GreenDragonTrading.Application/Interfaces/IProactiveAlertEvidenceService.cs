using GreenDragonTrading.Application.DTOs;

namespace GreenDragonTrading.Application.Interfaces
{
    /// <summary>
    /// Service for recording and querying proactive alert pipeline traces used as evidence.
    /// </summary>
    public interface IProactiveAlertEvidenceService
    {
        /// <summary>
        /// Appends a step to an existing trace. Creates the trace if it does not exist.
        /// Thread-safe via Redis SETNX + atomic operations.
        /// </summary>
        Task AppendStepAsync(
            string traceId,
            string ticker,
            string step,
            string result,
            string? failReason = null,
            Dictionary<string, object?>? detail = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Marks the final result and user counts on the trace.
        /// </summary>
        Task FinalizeTraceAsync(
            string traceId,
            string finalResult,
            int candidateUserCount,
            int eligibleUserCount,
            int notifiedUserCount,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a single trace by ID.
        /// </summary>
        Task<ProactiveAlertTraceDto?> GetTraceAsync(string traceId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a list of trace summaries matching the given filters with pagination.
        /// </summary>
        Task<(List<ProactiveAlertTraceSummaryDto> Items, int TotalCount)> ListTracesAsync(
            ProactiveAlertTraceQueryDto query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Computes aggregate statistics for the evidence dashboard.
        /// </summary>
        Task<ProactiveAlertEvidenceStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
    }
}
