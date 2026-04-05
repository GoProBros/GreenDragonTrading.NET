using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Infrastructure.BackgroundWorkers
{
    public partial class SsiStreamingBackgroundService
    {
        // In-memory map of index code → display name, populated at startup from DB.
        // Key is always upper-cased (e.g. "VNINDEX", "VN30").
        private readonly Dictionary<string, string> _indexNameCache = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Populates <see cref="_indexNameCache"/> from the database so that live broadcasts
        /// can include a human-readable name without a DB round-trip per tick.
        /// </summary>
        private async Task InitializeIndexCacheAsync(IUnitOfWork uow, CancellationToken cancellationToken)
        {
            try
            {
                var indices = await uow.MarketIndices.GetAllAsync(cancellationToken);
                foreach (var idx in indices)
                {
                    _indexNameCache[idx.Code.ToUpperInvariant()] = idx.Name;
                }

                _logger.LogInformation(
                    "Index name cache initialized with {Count} entries",
                    _indexNameCache.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing index name cache");
            }
        }

        /// <summary>
        /// Handles MI channel messages: stores live index snapshot in Redis and broadcasts to clients.
        /// </summary>
        /// <param name="redis">Redis service abstraction.</param>
        /// <param name="response">Deserialized MI payload from SSI.</param>
        private async Task HandleIndexData(IRedisService redis, MarketIndexDataResponse? response)
        {
            if (response == null)
            {
                _logger.LogWarning("[ChannelIndex] Received null MI response – skipping.");
                return;
            }

            var resolvedCode = response.ResolvedCode;
            if (string.IsNullOrWhiteSpace(resolvedCode))
            {
                _logger.LogWarning(
                    "[ChannelIndex] MI response has no index code in known fields. " +
                    "RType={RType} Exchange={Exchange} IndexValue={IndexValue} " +
                    "Symbol={Symbol} IndexID={IndexID} IndexCode={IndexCode} ComGroupCode={ComGroupCode} Id={Id}.",
                    response.RType ?? response.Rtype,
                    response.Exchange,
                    response.IndexValue,
                    response.Symbol,
                    response.IndexID,
                    response.IndexCode,
                    response.ComGroupCode,
                    response.Id);
                return;
            }

            try
            {
                var code = resolvedCode.ToUpperInvariant();

                // Resolve display name from the startup cache; fall back to the code itself.
                _indexNameCache.TryGetValue(code, out var name);

                var dto = new LiveIndexDataDto
                {
                    Code         = code,
                    Name         = response.ResolvedName ?? name ?? code,
                    IndexValue   = response.IndexValue   ?? response.IndexValEst ?? 0,
                    Change       = response.Change       ?? 0,
                    RatioChange  = response.RatioChange  ?? 0,
                    RefIndex     = response.RefIndex     ?? response.PriorIndexValue ?? 0,
                    OpenIndex    = response.OpenIndex    ?? 0,
                    HighIndex    = response.HighIndex    ?? 0,
                    LowIndex     = response.LowIndex     ?? 0,
                    TotalTrade   = response.TotalTrade   ?? 0,
                    TotalMatchVol = response.ResolvedTotalMatchVol ?? 0,
                    TotalMatchVal = response.ResolvedTotalMatchVal ?? 0,
                    AdvanceCount  = response.ResolvedAdvanceCount ?? 0,
                    DeclineCount  = response.ResolvedDeclineCount ?? 0,
                    NoChangeCount = response.ResolvedNoChangeCount ?? 0,
                    CeilingCount  = response.Ceilings ?? 0,
                    FloorCount    = response.Floors ?? 0,
                    Exchange      = response.Exchange,
                    Timestamp    = DateTime.UtcNow,
                };

                // Persist current snapshot to Redis hash (key: INDEX:{CODE})
                var redisKey = RedisConstants.IndexData(code);
                QueueRedisStringWrite(redisKey, dto, expiry: TimeSpan.FromHours(24));

                // Append to intraday history list (newest-first, capped at 4000 points ≈ full trading day at 5s intervals).
                // Use SSI's own Time field (already VN local HH:mm:ss); fall back to server UTC+7 if missing.
                var intradayKey = RedisConstants.IndexIntraday(code);
                var vnNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                    TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                var historyPoint = new IndexHistoryPointDto
                {
                    Time  = !string.IsNullOrWhiteSpace(response.Time)
                                ? response.Time          // e.g. "10:22:40" already VN local
                                : vnNow.ToString("HH:mm:ss"),
                    Value = dto.IndexValue,
                };

                // Reset intraday history when first tick of a new VN trading date arrives.
                // This guarantees Redis keeps only today's index intraday points.
                var intradayDateKey = $"{intradayKey}:DATE";
                var currentTradingDate = vnNow.ToString("yyyyMMdd");
                var storedTradingDate = await redis.GetAsync<string>(intradayDateKey);

                if (!string.Equals(storedTradingDate, currentTradingDate, StringComparison.Ordinal))
                {
                    await redis.RemoveAsync(intradayKey);
                    await redis.SetAsync(intradayDateKey, currentTradingDate, expiry: TimeSpan.FromDays(3));
                    _logger.LogInformation(
                        "[ChannelIndex] Reset intraday key for {Code}. TradingDate {OldDate} -> {NewDate}",
                        code,
                        storedTradingDate ?? "<empty>",
                        currentTradingDate);
                }

                // Full trading day (9:00-11:30 + 13:00-15:00) at 5s intervals ≈ 3,240 points. Use 4,000 as buffer.
                await redis.ListPushTrimAsync(intradayKey, historyPoint, maxLength: 4000);

                // Queue SignalR broadcast to INDEX:{CODE} group
                QueueIndexBroadcast(dto);

                // _logger.LogInformation("[ChannelIndex] Queued MI data for {Code}: value={IndexValue} change={RatioChange:+0.00;-0.00}%",
                //     code, dto.IndexValue, dto.RatioChange);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling MI index data for symbol: {Symbol}", resolvedCode);
            }
        }
    }
}
