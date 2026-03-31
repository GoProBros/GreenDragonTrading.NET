using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Indicators.Queries.GetIndicatorSnapshotZScore
{
    /// <summary>
    /// Handles z-score normalization of the latest indicator snapshot from Redis.
    /// </summary>
    public class GetIndicatorSnapshotZScoreQueryHandler(
        IRedisService redisService,
        ILogger<GetIndicatorSnapshotZScoreQueryHandler> logger)
        : IRequestHandler<GetIndicatorSnapshotZScoreQuery, ApiResponse<IndicatorSnapshotZScoreDto>>
    {
        private readonly IRedisService _redisService = redisService;
        private readonly ILogger<GetIndicatorSnapshotZScoreQueryHandler> _logger = logger;

        public async Task<ApiResponse<IndicatorSnapshotZScoreDto>> Handle(
            GetIndicatorSnapshotZScoreQuery request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Ticker))
            {
                return ApiResponse<IndicatorSnapshotZScoreDto>.Failure("Ticker không hợp lệ");
            }

            if (string.IsNullOrWhiteSpace(request.Timeframe))
            {
                return ApiResponse<IndicatorSnapshotZScoreDto>.Failure("Timeframe không hợp lệ");
            }

            var key = RedisConstants.IndicatorsZScore(request.Ticker, request.Timeframe);
            var snapshot = await _redisService.GetHashAsync<IndicatorSnapshotZScoreDto>(key);

            if (snapshot == null)
            {
                return ApiResponse<IndicatorSnapshotZScoreDto>.Failure(
                    $"Không tìm thấy indicator z-score cho {request.Ticker} ({request.Timeframe})");
            }

            if (snapshot.CandleTime == default)
            {
                var candleTime = await _redisService.GetHashFieldAsync<DateTime?>(key, nameof(IndicatorSnapshotZScoreDto.CandleTime));
                snapshot.CandleTime = candleTime ?? default;
            }

            if (snapshot.CalculatedAt == default)
            {
                var calculatedAt = await _redisService.GetHashFieldAsync<DateTime?>(key, nameof(IndicatorSnapshotZScoreDto.CalculatedAt));
                snapshot.CalculatedAt = calculatedAt ?? default;
            }

            _logger.LogDebug("Returned z-score indicator snapshot for {Ticker} {Timeframe}", request.Ticker, request.Timeframe);

            return ApiResponse<IndicatorSnapshotZScoreDto>.Success(
                snapshot,
                $"Lấy indicator z-score cho {request.Ticker} thành công");
        }
    }
}
