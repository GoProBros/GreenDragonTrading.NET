using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Indicators.Queries.GetIndicatorSnapshot
{
    /// <summary>
    /// Handles reading the latest indicator snapshot from Redis.
    /// </summary>
    public class GetIndicatorSnapshotQueryHandler(
        IRedisService redisService,
        ILogger<GetIndicatorSnapshotQueryHandler> logger)
        : IRequestHandler<GetIndicatorSnapshotQuery, ApiResponse<IndicatorSnapshotDto>>
    {
        private readonly IRedisService _redisService = redisService;
        private readonly ILogger<GetIndicatorSnapshotQueryHandler> _logger = logger;

        public async Task<ApiResponse<IndicatorSnapshotDto>> Handle(
            GetIndicatorSnapshotQuery request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Ticker))
            {
                return ApiResponse<IndicatorSnapshotDto>.Failure("Ticker không hợp lệ");
            }

            if (string.IsNullOrWhiteSpace(request.Timeframe))
            {
                return ApiResponse<IndicatorSnapshotDto>.Failure("Timeframe không hợp lệ");
            }

            var key = RedisConstants.Indicators(request.Ticker, request.Timeframe);
            var snapshot = await _redisService.GetHashAsync<IndicatorSnapshotDto>(key);

            if (snapshot == null)
            {
                return ApiResponse<IndicatorSnapshotDto>.Failure(
                    $"Không tìm thấy indicator cho {request.Ticker} ({request.Timeframe})");
            }

            // DateTime fields might be serialized as JSON strings in hash values.
            if (snapshot.CandleTime == default)
            {
                var candleTime = await _redisService.GetHashFieldAsync<DateTime?>(key, nameof(IndicatorSnapshotDto.CandleTime));
                snapshot.CandleTime = candleTime ?? default;
            }

            if (snapshot.CalculatedAt == default)
            {
                var calculatedAt = await _redisService.GetHashFieldAsync<DateTime?>(key, nameof(IndicatorSnapshotDto.CalculatedAt));
                snapshot.CalculatedAt = calculatedAt ?? default;
            }

            _logger.LogDebug("Returned raw indicator snapshot for {Ticker} {Timeframe}", request.Ticker, request.Timeframe);

            return ApiResponse<IndicatorSnapshotDto>.Success(
                snapshot,
                $"Lấy indicator thô cho {request.Ticker} thành công");
        }
    }
}
