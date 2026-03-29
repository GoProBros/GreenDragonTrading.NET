using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.MarketIndex.Queries.GetLiveIndices
{
    /// <summary>
    /// Reads live market index snapshots from Redis (written by the SSI MI streaming channel handler).
    /// Returns an entry per requested code; silently omits codes that have no cached data yet.
    /// </summary>
    public class GetLiveIndicesQueryHandler(
        IRedisService redisService,
        ILogger<GetLiveIndicesQueryHandler> logger)
        : IRequestHandler<GetLiveIndicesQuery, ApiResponse<List<LiveIndexDataDto>>>
    {
        private readonly IRedisService _redisService = redisService;
        private readonly ILogger<GetLiveIndicesQueryHandler> _logger = logger;

        public async Task<ApiResponse<List<LiveIndexDataDto>>> Handle(
            GetLiveIndicesQuery request,
            CancellationToken cancellationToken)
        {
            if (request.Codes.Count == 0)
            {
                return ApiResponse<List<LiveIndexDataDto>>.Success(
                    [],
                    "Không có mã chỉ số nào được yêu cầu");
            }

            // Fetch all snapshots in parallel; index data is stored as Redis strings (SetAsync)
            var fetchTasks = request.Codes.Select(async code =>
            {
                var key = RedisConstants.IndexData(code);
                var dto = await _redisService.GetAsync<LiveIndexDataDto>(key);
                return (code, dto);
            });

            var fetched = await Task.WhenAll(fetchTasks);

            // Preserve requested order; silently omit codes with no cached data yet
            var result = new List<LiveIndexDataDto>(request.Codes.Count);
            foreach (var (code, dto) in fetched)
            {
                if (dto != null)
                {
                    result.Add(dto);
                }
                else
                {
                    _logger.LogDebug("No cached snapshot for index {Code}", code);
                }
            }

            return ApiResponse<List<LiveIndexDataDto>>.Success(result, "Lấy dữ liệu chỉ số thành công");
        }
    }
}
