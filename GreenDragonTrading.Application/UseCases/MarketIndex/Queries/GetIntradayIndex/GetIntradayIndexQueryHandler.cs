using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.MarketIndex.Queries.GetIntradayIndex
{
    /// <summary>
    /// Reads today's intraday price history for a market index from Redis.
    /// The Redis list is newest-first (LPUSH), so results are reversed before returning
    /// to give the frontend chronological order for sparkline charts.
    /// </summary>
    public class GetIntradayIndexQueryHandler(
        IRedisService redisService,
        ILogger<GetIntradayIndexQueryHandler> logger)
        : IRequestHandler<GetIntradayIndexQuery, ApiResponse<List<IndexHistoryPointDto>>>
    {
        private readonly IRedisService _redisService = redisService;
        private readonly ILogger<GetIntradayIndexQueryHandler> _logger = logger;

        public async Task<ApiResponse<List<IndexHistoryPointDto>>> Handle(
            GetIntradayIndexQuery request,
            CancellationToken cancellationToken)
        {
            var key = RedisConstants.IndexIntraday(request.Code);
            var points = await _redisService.ListRangeAsync<IndexHistoryPointDto>(key, request.MaxPoints);

            // Redis list is newest-first; reverse to chronological order for sparkline
            points.Reverse();

            // Collapse consecutive duplicates (same Time + Value)
            if (points.Count > 1)
            {
                var deduped = new List<IndexHistoryPointDto>(points.Count);
                IndexHistoryPointDto? last = null;

                foreach (var point in points)
                {
                    if (last != null && point.Time == last.Time && point.Value == last.Value)
                    {
                        continue;
                    }

                    deduped.Add(point);
                    last = point;
                }

                points = deduped;
            }

            _logger.LogDebug("Returned {Count} intraday points for index {Code}", points.Count, request.Code);

            return ApiResponse<List<IndexHistoryPointDto>>.Success(
                points,
                $"Lấy lịch sử trong ngày của {request.Code} thành công");
        }
    }
}
