using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Infrastructure.Services;

/// <summary>
/// Service xử lý logic tính toán và aggregate dữ liệu heatmap
/// </summary>
public class HeatmapService : IHeatmapService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRedisService _redisService;
    private readonly ILogger<HeatmapService> _logger;

    public HeatmapService(
        IUnitOfWork unitOfWork,
        IRedisService redisService,
        ILogger<HeatmapService> logger)
    {
        _unitOfWork = unitOfWork;
        _redisService = redisService;
        _logger = logger;
    }

    private static readonly TimeSpan HeatmapCacheTtl = TimeSpan.FromSeconds(10);

    public async Task<HeatmapDataDto> GetHeatmapDataAsync(
        string? exchange = null,
        string? sector = null,
        string[]? tickers = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // ── Fast path: read pre-computed items directly from HEATMAP:{ticker} Redis keys ──
            if (tickers is { Length: > 0 })
            {
                return await GetHeatmapByTickersAsync(tickers);
            }

            // ── Standard path (full market) with response cache ──
            var cacheKey = RedisConstants.HeatmapResponse(exchange, sector);
            var cached = await _redisService.GetAsync<HeatmapDataDto>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("Returning cached heatmap data for exchange={Exchange}, sector={Sector}", exchange, sector);
                return cached;
            }

            _logger.LogInformation("Fetching heatmap data for exchange={Exchange}, sector={Sector}", exchange, sector);

            // 1. Lấy danh sách symbols active từ database.
            // Symbol chỉ link tới sector level 4; nếu caller truyền sector level 2,
            // cần expand thành tất cả level 4 IDs trước khi filter.
            List<string>? level4SectorIds = null;
            if (!string.IsNullOrWhiteSpace(sector))
            {
                level4SectorIds = await _unitOfWork.Sectors.GetAllChildLevel4SectorIdsAsync(
                    sector, cancellationToken);

                // If the sector itself is already level 4, include it directly
                if (level4SectorIds.Count == 0)
                    level4SectorIds.Add(sector);
            }

            var symbols = await _unitOfWork.Symbols.GetActiveSymbolsForHeatmapAsync(
                exchange,
                sectorIds: level4SectorIds,
                cancellationToken);

            if (!symbols.Any())
            {
                _logger.LogWarning("No active symbols found for exchange={Exchange}, sector={Sector}", exchange, sector);
                return new HeatmapDataDto
                {
                    Exchange = exchange,
                    Sector = sector,
                    Items = new List<HeatmapItemDto>(),
                    Timestamp = DateTime.UtcNow
                };
            }

            // 2. Load all sectors into a flat dict to walk up the parent hierarchy
            //    and resolve the level-2 ancestor name for each symbol.
            var (allSectors, _) = await _unitOfWork.Sectors.GetSectorsWithSymbolsAsync(
                level: null, status: null, pageIndex: 1, pageSize: 10000, cancellationToken);
            var sectorDict = allSectors.ToDictionary(s => s.Id);

            _logger.LogInformation("Found {Count} symbols to fetch market data", symbols.Count());

            // Fix #2: Fetch market data from Redis using BATCH operation
            // Reduces 1800 sequential calls to 1 pipeline call
            var marketDataItems = new List<HeatmapItemDto>();
            var redisKeys = symbols.Select(s => RedisConstants.MarketDataSymbol(s.Ticker)).ToList();
            
            var marketDataBatch = await _redisService.GetHashBatchAsync<MarketSymbolDto>(redisKeys);

            // Process all market data
            foreach (var symbol in symbols)
            {
                try
                {
                    var redisKey = RedisConstants.MarketDataSymbol(symbol.Ticker);
                    if (!marketDataBatch.TryGetValue(redisKey, out var marketData) || marketData == null)
                    {
                        continue;
                    }

                    // Calculate changePercent and changeValue
                    var currentPrice = (decimal)marketData.LastPrice;
                    var referencePrice = (decimal)marketData.ReferencePrice;

                    if (referencePrice <= 0)
                    {
                        continue;
                    }

                    var changeValue = currentPrice - referencePrice;
                    var changePercent = (changeValue / referencePrice) * 100;

                    // Resolve level-2 sector ancestor for grouping in the heatmap.
                    // Symbols are linked to level-4 sectors; walk up until level == 2.
                    var level2Sector = GetLevel2Ancestor(symbol.SectorId, sectorDict);
                    var sectorName = level2Sector?.ViName ?? level2Sector?.EnName
                                     ?? symbol.Sector?.ViName ?? symbol.Sector?.EnName
                                     ?? symbol.SectorId ?? "Khác";

                    // Map to HeatmapItemDto
                    var item = new HeatmapItemDto
                    {
                        Ticker = symbol.Ticker,
                        CompanyName = symbol.ViCompanyName ?? symbol.EnCompanyName ?? symbol.Ticker,
                        CurrentPrice = currentPrice,
                        ChangePercent = changePercent,
                        ChangeValue = changeValue,
                        Volume = (long)marketData.TotalVol,
                        TotalValue = (decimal)marketData.TotalVal,
                        Exchange = symbol.ExchangeCode,
                        Sector = level2Sector?.Id ?? symbol.SectorId,
                        SectorName = sectorName,
                        ColorType = GetHeatmapColor(changePercent),
                        LastUpdate = DateTime.UtcNow
                    };

                    marketDataItems.Add(item);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing market data for {Ticker}", symbol.Ticker);
                }
            }

            _logger.LogInformation("Successfully processed {Count} heatmap items", marketDataItems.Count);

            // 5. Build response, cache, and return
            var result = new HeatmapDataDto
            {
                Exchange = exchange,
                Sector = sector,
                Items = marketDataItems,
                Timestamp = DateTime.UtcNow
            };

            // Cache response in Redis for 10 seconds
            await _redisService.SetAsync(cacheKey, result, HeatmapCacheTtl);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching heatmap data for exchange={Exchange}, sector={Sector}", 
                exchange, sector);
            throw;
        }
    }

    /// <summary>
    /// Fast path: reads pre-computed HeatmapItemDto directly from HEATMAP:{ticker} Redis keys.
    /// No DB queries — background service writes these on every SSI streaming tick.
    /// </summary>
    private async Task<HeatmapDataDto> GetHeatmapByTickersAsync(string[] tickers)
    {
        _logger.LogInformation("Fast-path heatmap for {Count} tickers", tickers.Length);

        var items = new List<HeatmapItemDto>(tickers.Length);
        var tasks = tickers.Select(async ticker =>
        {
            var key = RedisConstants.Heatmap(ticker);
            return await _redisService.GetAsync<HeatmapItemDto>(key);
        });

        var results = await Task.WhenAll(tasks);
        foreach (var item in results)
        {
            if (item != null)
                items.Add(item);
        }

        _logger.LogInformation("Fast-path returned {Count}/{Total} heatmap items", items.Count, tickers.Length);

        return new HeatmapDataDto
        {
            Items = items,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Walks up the sector parent chain and returns the first sector at level 2.
    /// Returns null if no level-2 ancestor is found.
    /// </summary>
    private static Domain.Entities.Sector? GetLevel2Ancestor(
        string? sectorId,
        Dictionary<string, Domain.Entities.Sector> sectorDict)
    {
        if (string.IsNullOrWhiteSpace(sectorId)) return null;

        var current = sectorDict.GetValueOrDefault(sectorId);
        while (current != null)
        {
            if (current.Level == 2) return current;
            if (current.ParentId == null) break;
            current = sectorDict.GetValueOrDefault(current.ParentId);
        }
        return null;
    }

    public string GetHeatmapColor(decimal changePercent)
    {
        return changePercent switch
        {
            >= 6.5m => "ceiling",      // Tím (trần)
            >= 3.0m => "strong-up",    // Xanh đậm
            >= 1.0m => "up",           // Xanh nhạt
            >= -1.0m => "neutral",     // Vàng
            >= -3.0m => "down",        // Đỏ nhạt
            >= -6.5m => "strong-down", // Đỏ đậm
            _ => "floor"               // Xanh dương (sàn)
        };
    }
}
