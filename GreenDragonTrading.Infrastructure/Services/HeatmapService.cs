using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
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

    public async Task<HeatmapDataDto> GetHeatmapDataAsync(
        string? exchange = null,
        string? sector = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching heatmap data for exchange={Exchange}, sector={Sector}", exchange, sector);

            // 1. Lấy danh sách symbols active từ database
            var symbols = await _unitOfWork.Symbols.GetActiveSymbolsForHeatmapAsync(
                exchange,
                sector,
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

            var tickers = symbols.Select(s => s.Ticker).ToList();
            _logger.LogInformation("Found {Count} symbols to fetch market data", tickers.Count);

            // 2. Lấy market data từ Redis cho các symbols
            var marketDataItems = new List<HeatmapItemDto>();

            foreach (var symbol in symbols)
            {
                try
                {
                    var marketData = await _redisService.GetHashAsync<MarketSymbolDto>(
                        $"MarketData:Symbol:{symbol.Ticker}");

                    if (marketData == null)
                    {
                        _logger.LogDebug("No market data found for {Ticker}", symbol.Ticker);
                        continue;
                    }

                    // 3. Calculate changePercent and changeValue
                    var currentPrice = (decimal)marketData.LastPrice;
                    var referencePrice = (decimal)marketData.ReferencePrice;

                    if (referencePrice <= 0)
                    {
                        _logger.LogDebug("Invalid reference price for {Ticker}: {ReferencePrice}", 
                            symbol.Ticker, referencePrice);
                        continue;
                    }

                    var changeValue = currentPrice - referencePrice;
                    var changePercent = (changeValue / referencePrice) * 100;

                    // 4. Map to HeatmapItemDto
                    var item = new HeatmapItemDto
                    {
                        Ticker = symbol.Ticker,
                        CompanyName = symbol.ViCompanyName ?? symbol.EnCompanyName ?? symbol.Ticker,
                        CurrentPrice = currentPrice,
                        ChangePercent = changePercent,
                        ChangeValue = changeValue,
                        Volume = (long)marketData.TotalVol,
                        Exchange = symbol.ExchangeCode,
                        Sector = symbol.SectorId,
                        SectorName = symbol.Sector?.ViName ?? symbol.Sector?.EnName ?? symbol.SectorId,
                        ColorType = GetHeatmapColor(changePercent),
                        LastUpdate = DateTime.UtcNow
                    };

                    marketDataItems.Add(item);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing market data for {Ticker}", symbol.Ticker);
                }
            }

            _logger.LogInformation("Successfully processed {Count} heatmap items", marketDataItems.Count);

            // 5. Return HeatmapDataDto
            return new HeatmapDataDto
            {
                Exchange = exchange,
                Sector = sector,
                Items = marketDataItems,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching heatmap data for exchange={Exchange}, sector={Sector}", 
                exchange, sector);
            throw;
        }
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
