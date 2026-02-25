using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.WatchLists.Queries.GetMyWatchLists;

/// <summary>
/// Handler for GetMyWatchListsQuery
/// </summary>
public class GetMyWatchListsQueryHandler : IRequestHandler<GetMyWatchListsQuery, ApiResponse<List<WatchListListItemDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetMyWatchListsQueryHandler> _logger;

    public GetMyWatchListsQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        ILogger<GetMyWatchListsQueryHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<List<WatchListListItemDto>>> Handle(
        GetMyWatchListsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();

        var watchLists = await _uow.WatchLists.GetByUserIdAsync(userId, cancellationToken);

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

        _logger.LogInformation("Successfully retrieved {Count} watch lists for user {UserId}", result.Count, userId);

        return ApiResponse<List<WatchListListItemDto>>.Success(result, "Lấy danh sách watchlist thành công");
    }
}
