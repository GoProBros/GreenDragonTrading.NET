using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Queries.GetWatchListTopInterestedSymbols;

/// <summary>
/// Handler for GetWatchListTopInterestedSymbolsQuery.
/// </summary>
public class GetWatchListTopInterestedSymbolsQueryHandler : IRequestHandler<GetWatchListTopInterestedSymbolsQuery, ApiResponse<WatchListTopInterestedSymbolsDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<GetWatchListTopInterestedSymbolsQueryHandler> _logger;

    public GetWatchListTopInterestedSymbolsQueryHandler(
        IUnitOfWork uow,
        ILogger<GetWatchListTopInterestedSymbolsQueryHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<ApiResponse<WatchListTopInterestedSymbolsDto>> Handle(
        GetWatchListTopInterestedSymbolsQuery request,
        CancellationToken cancellationToken)
    {
        var activeCustomerIds = (await _uow.Users.GetActiveUsersAsync(cancellationToken))
            .Where(user => user.Role == UserRole.User)
            .Select(user => user.Id)
            .ToHashSet();

        var watchLists = (await _uow.WatchLists.GetAllAsync(cancellationToken))
            .Where(watchList => watchList.Status == CommonStatus.Active && activeCustomerIds.Contains(watchList.UserId))
            .ToList();

        var symbolCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var totalTickerOccurrences = 0;

        foreach (var watchList in watchLists)
        {
            foreach (var ticker in ParseTickers(watchList.Tickers, watchList.Id))
            {
                totalTickerOccurrences++;
                if (symbolCounts.TryGetValue(ticker, out var currentCount))
                {
                    symbolCounts[ticker] = currentCount + 1;
                }
                else
                {
                    symbolCounts[ticker] = 1;
                }
            }
        }

        var sortedSymbols = symbolCounts
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => new InterestedSymbolCountDto
            {
                Symbol = pair.Key,
                Count = pair.Value
            })
            .ToList();

        var result = new WatchListTopInterestedSymbolsDto
        {
            ActiveCustomerCount = activeCustomerIds.Count,
            ProcessedWatchListCount = watchLists.Count,
            TotalTickerOccurrences = totalTickerOccurrences,
            Top5Symbols = sortedSymbols.Take(5).ToList(),
            Top10Symbols = sortedSymbols.Take(10).ToList()
        };

        _logger.LogInformation(
            "Top interested symbols retrieved: ActiveCustomers={ActiveCustomers}, ProcessedWatchLists={ProcessedWatchLists}, UniqueSymbols={UniqueSymbols}",
            result.ActiveCustomerCount,
            result.ProcessedWatchListCount,
            symbolCounts.Count);

        return ApiResponse<WatchListTopInterestedSymbolsDto>.Success(result, "Lấy top mã quan tâm thành công.");
    }

    private IEnumerable<string> ParseTickers(string tickersJson, int watchListId)
    {
        if (string.IsNullOrWhiteSpace(tickersJson))
        {
            return Enumerable.Empty<string>();
        }

        try
        {
            using var document = JsonDocument.Parse(tickersJson);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                _logger.LogWarning(
                    "Unexpected watch list tickers format for watch list {WatchListId}. Expected JSON array.",
                    watchListId);
                return Enumerable.Empty<string>();
            }

            var result = new List<string>();
            foreach (var item in document.RootElement.EnumerateArray())
            {
                var ticker = ExtractTicker(item);
                if (string.IsNullOrWhiteSpace(ticker))
                {
                    continue;
                }

                result.Add(ticker.Trim().ToUpperInvariant());
            }

            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse tickers JSON for watch list {WatchListId}", watchListId);
            return Enumerable.Empty<string>();
        }
    }

    private static string? ExtractTicker(JsonElement item)
    {
        if (item.ValueKind == JsonValueKind.String)
        {
            return item.GetString();
        }

        if (item.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (TryGetStringProperty(item, "ticker", out var ticker) ||
            TryGetStringProperty(item, "symbol", out ticker) ||
            TryGetStringProperty(item, "code", out ticker))
        {
            return ticker;
        }

        return null;
    }

    private static bool TryGetStringProperty(JsonElement item, string propertyName, out string? value)
    {
        foreach (var property in item.EnumerateObject())
        {
            if (!property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (property.Value.ValueKind == JsonValueKind.String)
            {
                value = property.Value.GetString();
                return true;
            }

            break;
        }

        value = null;
        return false;
    }
}
