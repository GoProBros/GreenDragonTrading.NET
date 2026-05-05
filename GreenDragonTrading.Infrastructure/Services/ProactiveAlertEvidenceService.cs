using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Infrastructure.Services
{
    /// <summary>
    /// Redis-backed implementation of proactive alert evidence recording.
    ///
    /// Storage design:
    /// - Each trace is a string key:  evidence:trace:{traceId}  → JSON of ProactiveAlertTraceDto (TTL 7 days)
    /// - Timeline sorted set:        evidence:timeline          → member = traceId, score = CreatedAt ticks
    /// - Ticker index sorted set:    evidence:ticker:{TICKER}   → member = traceId, score = CreatedAt ticks
    ///
    /// All operations use Redis SETNX / JSON patch to be thread-safe across multiple workers.
    /// </summary>
    public class ProactiveAlertEvidenceService : IProactiveAlertEvidenceService
    {
        private readonly IRedisService _redis;
        private readonly ILogger<ProactiveAlertEvidenceService> _logger;
        private static readonly TimeSpan Ttl = RedisConstants.EvidenceTtl();

        public ProactiveAlertEvidenceService(IRedisService redis, ILogger<ProactiveAlertEvidenceService> logger)
        {
            _redis = redis;
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
                var traceKey = RedisConstants.EvidenceTrace(traceId);
                var timelineKey = RedisConstants.EvidenceTimeline();
                var tickerIndexKey = RedisConstants.EvidenceTickerIndex(ticker);

                // Attempt to load existing trace
                var existing = await _redis.GetAsync<ProactiveAlertTraceDto>(traceKey);
                ProactiveAlertTraceDto trace;

                if (existing != null)
                {
                    trace = existing;
                }
                else
                {
                    trace = new ProactiveAlertTraceDto
                    {
                        TraceId = traceId,
                        Ticker = ticker,
                        CreatedAt = DateTime.UtcNow,
                    };

                    // Add to timeline index (newest-first: score = -CreatedAt.Ticks)
                    await _redis.SortedSetAddAsync(timelineKey, traceId, -trace.CreatedAt.Ticks);

                    // Add to ticker index
                    await _redis.SortedSetAddAsync(tickerIndexKey, traceId, -trace.CreatedAt.Ticks);
                }

                trace.Steps.Add(new ProactiveAlertTraceStepDto
                {
                    Step = step,
                    Result = result,
                    FailReason = failReason,
                    Detail = detail,
                    Timestamp = DateTime.UtcNow,
                });

                trace.UpdatedAt = DateTime.UtcNow;

                // Persist
                await _redis.SetAsync(traceKey, trace, Ttl);
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
                var traceKey = RedisConstants.EvidenceTrace(traceId);
                var trace = await _redis.GetAsync<ProactiveAlertTraceDto>(traceKey);

                if (trace == null)
                {
                    _logger.LogWarning("Cannot finalize trace {TraceId}: not found", traceId);
                    return;
                }

                trace.FinalResult = finalResult;
                trace.CandidateUserCount = candidateUserCount;
                trace.EligibleUserCount = eligibleUserCount;
                trace.NotifiedUserCount = notifiedUserCount;
                trace.UpdatedAt = DateTime.UtcNow;

                await _redis.SetAsync(traceKey, trace, Ttl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to finalize evidence trace {TraceId}", traceId);
            }
        }

        public async Task<ProactiveAlertTraceDto?> GetTraceAsync(string traceId, CancellationToken cancellationToken = default)
        {
            var traceKey = RedisConstants.EvidenceTrace(traceId);
            return await _redis.GetAsync<ProactiveAlertTraceDto>(traceKey);
        }

        public async Task<(List<ProactiveAlertTraceSummaryDto> Items, int TotalCount)> ListTracesAsync(
            ProactiveAlertTraceQueryDto query,
            CancellationToken cancellationToken = default)
        {
            var timelineKey = RedisConstants.EvidenceTimeline();

            // Fetch all trace IDs from timeline (negative score = newest first, ascending range)
            var members = await _redis.SortedSetRangeByScoreAsync(
                timelineKey,
                double.NegativeInfinity,
                double.PositiveInfinity);

            if (members == null || members.Count == 0)
            {
                return (new List<ProactiveAlertTraceSummaryDto>(), 0);
            }

            var summaries = new List<ProactiveAlertTraceSummaryDto>();
            var pageIndex = Math.Max(0, query.PageIndex);
            var pageSize = Math.Clamp(query.PageSize, 1, 200);

            foreach (var member in members)
            {
                if (summaries.Count >= (pageIndex + 1) * pageSize + pageSize) // fetched enough for filtering
                {
                    break;
                }

                var trace = await GetTraceAsync(member, cancellationToken);
                if (trace == null) continue;

                // Apply filters
                if (!string.IsNullOrWhiteSpace(query.Ticker) &&
                    !trace.Ticker.Contains(query.Ticker, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!string.IsNullOrWhiteSpace(query.FinalResult) &&
                    !string.Equals(trace.FinalResult, query.FinalResult, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (query.From.HasValue && trace.CreatedAt < query.From.Value)
                    continue;

                if (query.To.HasValue && trace.CreatedAt > query.To.Value)
                    continue;

                summaries.Add(new ProactiveAlertTraceSummaryDto
                {
                    TraceId = trace.TraceId,
                    Ticker = trace.Ticker,
                    CreatedAt = trace.CreatedAt,
                    UpdatedAt = trace.UpdatedAt,
                    FinalResult = trace.FinalResult,
                    StepCount = trace.Steps.Count,
                    CandidateUserCount = trace.CandidateUserCount,
                    EligibleUserCount = trace.EligibleUserCount,
                    NotifiedUserCount = trace.NotifiedUserCount,
                });
            }

            var totalCount = summaries.Count;
            var paged = summaries.Skip(pageIndex * pageSize).Take(pageSize).ToList();

            return (paged, totalCount);
        }

        public async Task<ProactiveAlertEvidenceStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
        {
            var stats = new ProactiveAlertEvidenceStatsDto();
            var timelineKey = RedisConstants.EvidenceTimeline();

            var members = await _redis.SortedSetRangeByScoreAsync(
                timelineKey,
                double.NegativeInfinity,
                double.PositiveInfinity);

            if (members == null || members.Count == 0)
            {
                return stats;
            }

            var byFinalResult = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var byTicker = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var byHour = new Dictionary<int, int>();
            long totalAiLatencyMs = 0;

            DateTime? oldest = null;
            DateTime? newest = null;

            foreach (var member in members)
            {
                var trace = await GetTraceAsync(member, cancellationToken);
                if (trace == null) continue;

                stats.TotalTraces++;

                // By final result
                var fr = string.IsNullOrWhiteSpace(trace.FinalResult) ? "pending" : trace.FinalResult;
                byFinalResult.TryGetValue(fr, out var frCount);
                byFinalResult[fr] = frCount + 1;

                // By ticker
                byTicker.TryGetValue(trace.Ticker, out var tCount);
                byTicker[trace.Ticker] = tCount + 1;

                // By hour (UTC)
                var hour = trace.CreatedAt.Hour;
                byHour.TryGetValue(hour, out var hCount);
                byHour[hour] = hCount + 1;

                // AI stats
                var aiStep = trace.Steps.FirstOrDefault(s => s.Step == "ai_evaluation");
                if (aiStep != null)
                {
                    stats.AiEvaluationAttempted++;
                    if (aiStep.Result == "success")
                    {
                        stats.AiEvaluationSuccess++;
                        if (aiStep.Detail != null &&
                            aiStep.Detail.TryGetValue("latencyMs", out var latObj) &&
                            latObj is JsonElement latElem &&
                            latElem.TryGetInt32(out var latMs))
                        {
                            totalAiLatencyMs += latMs;
                        }
                    }
                    else if (aiStep.Result == "fallback")
                    {
                        stats.AiEvaluationFallback++;
                    }
                    else
                    {
                        stats.AiEvaluationFailed++;
                    }
                }

                // Notified users
                stats.TotalNotifiedUsers += trace.NotifiedUserCount;

                // Time range
                if (oldest == null || trace.CreatedAt < oldest.Value)
                    oldest = trace.CreatedAt;
                if (newest == null || trace.CreatedAt > newest.Value)
                    newest = trace.CreatedAt;
            }

            stats.ByFinalResult = byFinalResult;
            stats.TopTickers = byTicker.OrderByDescending(kv => kv.Value).Take(20)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
            stats.ByHour = byHour;
            stats.OldestTrace = oldest;
            stats.NewestTrace = newest;

            if (stats.AiEvaluationSuccess > 0)
            {
                stats.AvgAiLatencyMs = Math.Round((double)totalAiLatencyMs / stats.AiEvaluationSuccess, 1);
            }

            return stats;
        }
    }
}
