using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.WatchLists.Queries.GetAllWatchListSymbols;

/// <summary>
/// Handler for GetAllWatchListSymbolsQuery. Retrieves all watchlists of the current user,
/// extracts tickers from each, and returns a deduplicated list of symbols.
/// </summary>
public class GetAllWatchListSymbolsQueryHandler : IRequestHandler<GetAllWatchListSymbolsQuery, ApiResponse<List<string>>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetAllWatchListSymbolsQueryHandler> _logger;

    public GetAllWatchListSymbolsQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        ILogger<GetAllWatchListSymbolsQueryHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<List<string>>> Handle(
        GetAllWatchListSymbolsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();

        var watchLists = await _uow.WatchLists.GetByUserIdAsync(userId, cancellationToken);

        var allSymbols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var wl in watchLists)
        {
            try
            {
                var tickers = JsonSerializer.Deserialize<JsonElement>(wl.Tickers);
                if (tickers.ValueKind == JsonValueKind.Array)
                {
                    foreach (var ticker in tickers.EnumerateArray())
                    {
                        var symbol = ticker.GetString();
                        if (!string.IsNullOrWhiteSpace(symbol))
                        {
                            allSymbols.Add(symbol.Trim());
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse tickers JSON for watchlist {WatchListId}", wl.Id);
            }
        }

        var result = allSymbols.ToList();
        result.Sort(); // Sort alphabetically for consistent output

        _logger.LogInformation(
            "Retrieved {SymbolCount} unique symbols from {WatchListCount} watchlists for user {UserId}",
            result.Count, watchLists.Count, userId);

        return ApiResponse<List<string>>.Success(result, "Lấy danh sách mã chứng khoán thành công");
    }
}
