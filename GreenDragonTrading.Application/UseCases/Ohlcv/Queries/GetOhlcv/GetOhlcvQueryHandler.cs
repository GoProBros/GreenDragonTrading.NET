using GreenDragonTrading.Application.Common.Extensions;
using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Common.Utils;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.Ohlcv.Queries.GetOhlcv
{
    public class GetOhlcvQueryHandler
        : IRequestHandler<GetOhlcvQuery, ApiResponse<OhlcvResponseDto>>
    {
        private readonly IOhlcvUnitOfWork _ohlcvUow;
        private readonly IRedisService _redisService;
        private readonly ILogger<GetOhlcvQueryHandler> _logger;

        public GetOhlcvQueryHandler(
            IOhlcvUnitOfWork ohlcvUow,
            IRedisService redisService,
            ILogger<GetOhlcvQueryHandler> logger)
        {
            _ohlcvUow = ohlcvUow;
            _redisService = redisService;
            _logger = logger;
        }

        public async Task<ApiResponse<OhlcvResponseDto>> Handle(
            GetOhlcvQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Validate timeframe
                if (!OhlcvConstants.Timeframes.IsValid(request.Timeframe))
                {
                    return ApiResponse<OhlcvResponseDto>.Failure(
                        $"Timeframe không hợp lệ. Phải là một trong: {string.Join(", ", OhlcvConstants.Timeframes.All)}"
                    );
                }

                _logger.LogInformation("Querying OHLCV for {Ticker}, Timeframe: {Timeframe}, From: {From}, To: {To}",
                    request.Ticker, request.Timeframe, request.FromTime, request.ToTime);

                // 1. Check cache chỉ cho computed timeframes (không cache M1/D1 raw data)
                // Lý do: M1/D1 query từ TimescaleDB rất nhanh, computed timeframes tốn CPU
                var isComputedTimeframe = !OhlcvConstants.Timeframes.IsStored(request.Timeframe);
                if (request.UseCache && isComputedTimeframe)
                {
                    var cachedResult = await GetFromCache(request, cancellationToken);
                    if (cachedResult != null)
                    {
                        _logger.LogInformation("OHLCV data found in cache for {Ticker} (computed timeframe)", request.Ticker);
                        return ApiResponse<OhlcvResponseDto>.Success(cachedResult, "Lấy data từ cache");
                    }
                }

                // 2. Kiểm tra xem timeframe có trong DB không
                List<Domain.Entities.Ohlcv> ohlcvData;
                string source;

                if (OhlcvConstants.Timeframes.IsStored(request.Timeframe))
                {
                    // Timeframe có trong DB (M1 hoặc D1) - query trực tiếp
                    ohlcvData = await QueryFromDatabase(request, cancellationToken);
                    source = "Database";
                }
                else
                {
                    // Timeframe cần tính toán (M5, M15, H1, H4, W1, MN1)
                    ohlcvData = await ComputeTimeframe(request, cancellationToken);
                    source = "Computed";
                }

                // 3. Convert sang DTO
                var response = new OhlcvResponseDto
                {
                    Ticker = request.Ticker.ToUpper(),
                    Timeframe = request.Timeframe,
                    Count = ohlcvData.Count,
                    Data = ohlcvData.ToDtoList(),
                    Source = source
                };

                if (ohlcvData.Any())
                {
                    response.FirstTime = ohlcvData.First().Time;
                    response.LastTime = ohlcvData.Last().Time;
                }

                // 4. Cache kết quả chỉ cho computed timeframes
                // M1/D1 không cache vì query nhanh và có thể bị outdated khi import
                if (request.UseCache && isComputedTimeframe && ohlcvData.Any())
                {
                    await SaveToCache(request, response, cancellationToken);
                }

                _logger.LogInformation("Retrieved {Count} {Timeframe} candles for {Ticker}",
                    response.Count, request.Timeframe, request.Ticker);

                return ApiResponse<OhlcvResponseDto>.Success(
                    response,
                    $"Lấy thành công {response.Count} nến {request.Timeframe}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying OHLCV for {Ticker}", request.Ticker);

                return ApiResponse<OhlcvResponseDto>.Failure(
                    $"Lỗi khi lấy OHLCV data: {ex.Message}"
                );
            }
        }

        private async Task<List<Domain.Entities.Ohlcv>> QueryFromDatabase(
            GetOhlcvQuery request,
            CancellationToken cancellationToken)
        {
            if (request.Limit.HasValue)
            {
                // Lấy N nến mới nhất
                var candles = await _ohlcvUow.Ohlcv.GetLatestCandlesAsync(
                    request.Ticker,
                    request.Timeframe,
                    request.Limit.Value,
                    cancellationToken
                );

                // Reverse để trả về theo thứ tự time tăng dần
                return candles.OrderBy(c => c.Time).ToList();
            }
            else
            {
                // Lấy theo time range
                return await _ohlcvUow.Ohlcv.GetByTickerAndTimeRangeAsync(
                    request.Ticker,
                    request.Timeframe,
                    request.FromTime,
                    request.ToTime,
                    cancellationToken
                );
            }
        }

        private async Task<List<Domain.Entities.Ohlcv>> ComputeTimeframe(
            GetOhlcvQuery request,
            CancellationToken cancellationToken)
        {
            // Xác định source timeframe để aggregate
            string sourceTimeframe;

            if (OhlcvConstants.Timeframes.ComputedFromM1.Contains(request.Timeframe))
            {
                // M5, M15, M30, H1, H4 → tính từ M1
                sourceTimeframe = OhlcvConstants.Timeframes.M1;
            }
            else if (OhlcvConstants.Timeframes.ComputedFromD1.Contains(request.Timeframe))
            {
                // W1, MN1 → tính từ D1
                sourceTimeframe = OhlcvConstants.Timeframes.D1;
            }
            else
            {
                throw new InvalidOperationException($"Cannot compute timeframe {request.Timeframe}");
            }

            // Query source data từ DB
            var sourceData = await _ohlcvUow.Ohlcv.GetByTickerAndTimeRangeAsync(
                request.Ticker,
                sourceTimeframe,
                request.FromTime,
                request.ToTime,
                cancellationToken
            );

            if (!sourceData.Any())
            {
                _logger.LogWarning("No {Source} data found for {Ticker} to compute {Target}",
                    sourceTimeframe, request.Ticker, request.Timeframe);
                return new List<Domain.Entities.Ohlcv>();
            }

            // Aggregate
            List<Domain.Entities.Ohlcv> aggregated;

            if (sourceTimeframe == OhlcvConstants.Timeframes.M1)
            {
                aggregated = OhlcvAggregationHelper.AggregateFromM1(sourceData, request.Timeframe);
            }
            else
            {
                aggregated = OhlcvAggregationHelper.AggregateFromD1(sourceData, request.Timeframe);
            }

            // Apply limit nếu có
            if (request.Limit.HasValue && aggregated.Count > request.Limit.Value)
            {
                aggregated = aggregated.TakeLast(request.Limit.Value).ToList();
            }

            return aggregated;
        }

        private async Task<OhlcvResponseDto?> GetFromCache(
            GetOhlcvQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var cacheKey = OhlcvConstants.CacheKeys.GetKey(
                    request.Ticker,
                    request.Timeframe,
                    request.FromTime
                );

                var cachedData = await _redisService.GetAsync<OhlcvResponseDto>(cacheKey);

                if (cachedData != null)
                {
                    return cachedData;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error reading from cache for {Ticker}", request.Ticker);
            }

            return null;
        }

        private async Task SaveToCache(
            GetOhlcvQuery request,
            OhlcvResponseDto response,
            CancellationToken cancellationToken)
        {
            try
            {
                var cacheKey = OhlcvConstants.CacheKeys.GetKey(
                    request.Ticker,
                    request.Timeframe,
                    request.FromTime
                );

                var expiration = OhlcvConstants.CacheExpiration.GetExpiration(request.Timeframe);

                await _redisService.SetAsync(cacheKey, response, expiration);

                _logger.LogDebug("Cached OHLCV data for {Ticker}, key: {Key}, expiration: {Exp}",
                    request.Ticker, cacheKey, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error saving to cache for {Ticker}", request.Ticker);
            }
        }
    }
}