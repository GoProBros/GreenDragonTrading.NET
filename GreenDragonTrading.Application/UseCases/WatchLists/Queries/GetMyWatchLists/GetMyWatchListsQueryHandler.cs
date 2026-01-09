using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.WatchLists.Queries.GetMyWatchLists;

public class GetMyWatchListsQueryHandler : IRequestHandler<GetMyWatchListsQuery, ApiResponse<List<WatchListListItemDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtService _jwtService;
    private readonly ILogger<GetMyWatchListsQueryHandler> _logger;

    public GetMyWatchListsQueryHandler(
        IUnitOfWork uow,
        IHttpContextAccessor httpContextAccessor,
        IJwtService jwtService,
        ILogger<GetMyWatchListsQueryHandler> logger)
    {
        _uow = uow;
        _httpContextAccessor = httpContextAccessor;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<ApiResponse<List<WatchListListItemDto>>> Handle(
        GetMyWatchListsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext ?? throw new UnauthenticatedException("Không tìm thấy HTTP context.");

            var authHeaderValue = httpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeaderValue) || !authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthenticatedException("Không tìm thấy Authorization header.");
            }

            var accessToken = authHeaderValue.Substring("Bearer ".Length).Trim();

            var tokenInfo = _jwtService.GetTokenInfo(accessToken) ?? throw new UnauthenticatedException("Access token không hợp lệ.");

            var user = await _uow.Users.GetByIdAsync(tokenInfo.UserId, cancellationToken) ?? throw new NotFoundException("Người dùng không tồn tại.");

            var watchLists = await _uow.WatchLists.GetByUserIdAsync(user.Id, cancellationToken);

            var result = watchLists.Select(w =>
            {
                int tickerCount = 0;
                try
                {
                    var tickers = JsonSerializer.Deserialize<JsonElement>(w.Tickers);
                    if (tickers.ValueKind == JsonValueKind.Array)
                    {
                        tickerCount = tickers.GetArrayLength();
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse tickers JSON for watch list {WatchListId}", w.Id);
                }

                return new WatchListListItemDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    TickerCount = tickerCount,
                    Status = w.Status,
                    CreatedAt = w.CreatedAt,
                    UpdatedAt = w.UpdatedAt
                };
            }).ToList();

            _logger.LogInformation("Successfully retrieved {Count} watch lists for user {UserId}", result.Count, user.Id);

            return ApiResponse<List<WatchListListItemDto>>.Success(result, "Lấy danh sách watchlist thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving watch lists");
            throw;
        }
    }
}
