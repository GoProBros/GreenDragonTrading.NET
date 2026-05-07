using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;

namespace GreenDragonTrading.Infrastructure.Services
{
    /// <summary>
    /// PostgreSQL-backed implementation of proactive alert evidence recording.
    ///
    /// Storage design:
    /// - Each trace is a row in proactive_alert_traces.
    /// - Steps are stored as JSONB within the row.
    /// - Indexes on tracer, final_result, and created_at for efficient querying.
    /// </summary>
    public class ProactiveAlertEvidenceService : IProactiveAlertEvidenceService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<ProactiveAlertEvidenceService> _logger;

        public ProactiveAlertEvidenceService(IUnitOfWork uow, ILogger<ProactiveAlertEvidenceService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task AppendStepAsync(
            string traceId,
            string ticker,
            string step,
            string result,
            string? failReason = null,
            Dictionary<string, object?>? detail = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = await _uow.ProactiveAlertTraces.FirstOrDefaultAsync(
                    e => e.TraceId == traceId,
                    cancellationToken);

                if (entity == null)
                {
                    entity = new Domain.Entities.ProactiveAlertTrace
                    {
                        TraceId = traceId,
                        Ticker = ticker,
                        CreatedAt = DateTime.UtcNow,
                    };
                    await _uow.ProactiveAlertTraces.AddAsync(entity, cancellationToken);
                }

                entity.Steps.Add(new ProactiveAlertTraceStep
                {
                    Step = step,
                    Result = result,
                    FailReason = failReason,
                    Detail = detail != null
                        ? detail.ToDictionary(kv => kv.Key, kv => kv.Value)
                        : null,
                    Timestamp = DateTime.UtcNow,
                });

                entity.UpdatedAt = DateTime.UtcNow;

                await _uow.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to append evidence step {Step} for trace {TraceId}", step, traceId);
            }
        }

        public async Task FinalizeTraceAsync(
            string traceId,
            string finalResult,
            int candidateUserCount,
            int eligibleUserCount,
            int notifiedUserCount,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = await _uow.ProactiveAlertTraces.FirstOrDefaultAsync(
                    e => e.TraceId == traceId,
                    cancellationToken);

                if (entity == null)
                {
                    _logger.LogWarning("Cannot finalize trace {TraceId}: not found", traceId);
                    return;
                }

                entity.FinalResult = finalResult;
                entity.CandidateUserCount = candidateUserCount;
                entity.EligibleUserCount = eligibleUserCount;
                entity.NotifiedUserCount = notifiedUserCount;
                entity.UpdatedAt = DateTime.UtcNow;

                await _uow.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to finalize evidence trace {TraceId}", traceId);
            }
        }

        public async Task<ProactiveAlertTraceDto?> GetTraceAsync(string traceId, CancellationToken cancellationToken = default)
        {
            var entity = await _uow.ProactiveAlertTraces.FirstOrDefaultAsync(
                e => e.TraceId == traceId,
                cancellationToken);

            if (entity == null)
            {
                return null;
            }

            return MapToDto(entity);
        }

        public async Task<(List<ProactiveAlertTraceSummaryDto> Items, int TotalCount)> ListTracesAsync(
            ProactiveAlertTraceQueryDto query,
            CancellationToken cancellationToken = default)
        {
            var queryable = _uow.ProactiveAlertTraces.GetQueryable();

            if (!string.IsNullOrWhiteSpace(query.Ticker))
            {
                queryable = queryable.Where(e => e.Ticker.Contains(query.Ticker));
            }

            if (!string.IsNullOrWhiteSpace(query.FinalResult))
            {
                queryable = queryable.Where(e => e.FinalResult == query.FinalResult);
            }

            if (query.From.HasValue)
            {
                queryable = queryable.Where(e => e.CreatedAt >= query.From.Value);
            }

            if (query.To.HasValue)
            {
                queryable = queryable.Where(e => e.CreatedAt <= query.To.Value);
            }

            var totalCount = queryable.Count();

            var pageIndex = Math.Max(0, query.PageIndex);
            var pageSize = Math.Clamp(query.PageSize, 1, 200);

            var entities = queryable
                .OrderByDescending(e => e.CreatedAt)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToList();

            var items = entities.Select(e => new ProactiveAlertTraceSummaryDto
            {
                TraceId = e.TraceId,
                Ticker = e.Ticker,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                FinalResult = e.FinalResult ?? "pending",
                StepCount = e.Steps.Count,
                CandidateUserCount = e.CandidateUserCount,
                EligibleUserCount = e.EligibleUserCount,
                NotifiedUserCount = e.NotifiedUserCount,
            }).ToList();

            return (items, totalCount);
        }

        public async Task<ProactiveAlertEvidenceStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
        {
            var stats = new ProactiveAlertEvidenceStatsDto();

            var allTraces = _uow.ProactiveAlertTraces.GetQueryable();
            var totalCount = allTraces.Count();

            if (totalCount == 0)
            {
                return stats;
            }

            stats.TotalTraces = totalCount;

            // Materialize basic fields first, then compute JSONB-derived fields in memory
            var raw = allTraces
                .Select(e => new
                {
                    e.Ticker,
                    FinalResult = string.IsNullOrWhiteSpace(e.FinalResult) ? "pending" : e.FinalResult,
                    e.CreatedAt,
                    e.NotifiedUserCount,
                    e.Steps,
                })
                .ToList();

            var summary = raw.Select(e => new
            {
                e.Ticker,
                e.FinalResult,
                Hour = e.CreatedAt.Hour,
                e.CreatedAt,
                e.NotifiedUserCount,
                HasAiStep = e.Steps.Any(s => s.Step == "ai_evaluation"),
            }).ToList();

            // By final result
            stats.ByFinalResult = summary
                .GroupBy(x => x.FinalResult)
                .ToDictionary(g => g.Key, g => g.Count());

            // Top tickers
            stats.TopTickers = summary
                .GroupBy(x => x.Ticker)
                .OrderByDescending(g => g.Count())
                .Take(20)
                .ToDictionary(g => g.Key, g => g.Count());

            // By hour (UTC)
            stats.ByHour = summary
                .GroupBy(x => x.Hour)
                .ToDictionary(g => g.Key, g => g.Count());

            // Oldest / newest
            stats.OldestTrace = summary.Min(x => (DateTime?)x.CreatedAt);
            stats.NewestTrace = summary.Max(x => (DateTime?)x.CreatedAt);

            // AI stats
            stats.AiEvaluationAttempted = summary.Count(x => x.HasAiStep);
            stats.TotalNotifiedUsers = summary.Sum(x => x.NotifiedUserCount);

            // Break down AI evaluation results from step data
            var aiSteps = raw
                .SelectMany(e => e.Steps
                    .Where(s => s.Step == "ai_evaluation")
                    .Select(s => new { Trace = e, Step = s }))
                .ToList();

            stats.AiEvaluationSuccess = aiSteps.Count(x => x.Step.Result == "success");
            stats.AiEvaluationFallback = aiSteps.Count(x => x.Step.Result == "fallback");
            stats.AiEvaluationFailed = aiSteps.Count(x => x.Step.Result is "failed" or "error");

            if (aiSteps.Count > 0)
            {
                var latencies = aiSteps
                    .Select(x => x.Step.Detail?.TryGetValue("latencyMs", out var v) == true
                        ? int.TryParse(v?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var ms) ? ms : (int?)null
                        : null)
                    .Where(x => x.HasValue)
                    .Select(x => x!.Value)
                    .ToList();

                stats.AvgAiLatencyMs = latencies.Count > 0 ? latencies.Average() : 0;
            }

            return stats;
        }

        private static ProactiveAlertTraceDto MapToDto(Domain.Entities.ProactiveAlertTrace entity)
        {
            return new ProactiveAlertTraceDto
            {
                TraceId = entity.TraceId,
                Ticker = entity.Ticker,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                FinalResult = entity.FinalResult ?? "pending",
                CandidateUserCount = entity.CandidateUserCount,
                EligibleUserCount = entity.EligibleUserCount,
                NotifiedUserCount = entity.NotifiedUserCount,
                Steps = entity.Steps.Select(s => new ProactiveAlertTraceStepDto
                {
                    Step = s.Step,
                    Result = s.Result,
                    FailReason = s.FailReason,
                    Detail = s.Detail,
                    Timestamp = s.Timestamp,
                }).ToList(),
            };
        }
    }
}
