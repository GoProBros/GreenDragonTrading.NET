using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Infrastructure.BackgroundWorkers
{
    /// <summary>
    /// Background service to check for price adjustments due to corporate actions (stock splits, dividends, etc.)
    /// Runs at 21:00 and 07:00 VN time (outside trading hours)
    /// Checks 3 random days in the last 3 months and re-imports historical data if price changes detected
    /// </summary>
    public class PriceAdjustmentCheckService : BackgroundService
    {
        private readonly ILogger<PriceAdjustmentCheckService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private DateTime _lastCheckTime = DateTime.MinValue;

        public PriceAdjustmentCheckService(
            ILogger<PriceAdjustmentCheckService> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🔍 Price Adjustment Check Service started.");

            // Check every 30 minutes
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(30));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    var vnTime = DateTime.UtcNow.AddHours(7);

                    // Run at 21:00 or 07:00 VN time (± 30 minutes window)
                    if ((vnTime.Hour == 21 || vnTime.Hour == 7) && vnTime.Minute < 30)
                    {
                        // Avoid running twice in the same hour
                        if ((DateTime.UtcNow - _lastCheckTime).TotalHours >= 1)
                        {
                            _logger.LogInformation(
                                "Starting price adjustment check at {Time} VN time (outside trading hours)",
                                vnTime.ToString("yyyy-MM-dd HH:mm"));

                            await CheckPriceAdjustmentsAsync(stoppingToken);

                            _lastCheckTime = DateTime.UtcNow;

                            _logger.LogInformation("Price adjustment check completed");

                            // Sleep 1 hour to avoid re-running in the same time window
                            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during price adjustment check cycle");
                }
            }
        }

        private async Task CheckPriceAdjustmentsAsync(CancellationToken ct)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var ssiService = scope.ServiceProvider.GetRequiredService<ISsiServiceV2>();
            var ohlcvRepository = scope.ServiceProvider.GetRequiredService<IOhlcvRepository>();
            var redisService = scope.ServiceProvider.GetRequiredService<IRedisService>();

            // Get all tickers
            var tickers = await unitOfWork.Symbols.GetAllTickersAsync(ct);

            // Generate 3 random dates in the last 3 months (only weekdays)
            var randomDates = GenerateRandomTradingDates(3, 90);

            _logger.LogInformation(
                "Checking price adjustments for {Count} tickers on dates: {Dates}",
                tickers.Count(), string.Join(", ", randomDates.Select(d => d.ToString("yyyy-MM-dd"))));

            foreach (var ticker in tickers)
            {
                try
                {
                    bool priceChanged = await CheckTickerPriceChangedAsync(
                        ticker, randomDates, ssiService, ohlcvRepository, ct);

                    if (priceChanged)
                    {
                        _logger.LogWarning(
                            "🔄 Corporate action detected for {Ticker}. Re-importing ALL historical data (M1 + D1)...",
                            ticker);

                        await ReImportAllHistoricalDataAsync(
                            ticker, ssiService, ohlcvRepository, redisService, ct);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking price adjustment for {Ticker}", ticker);
                }

                // Delay between tickers to avoid SSI API rate limiting
                await Task.Delay(100, ct);
            }
        }

        private List<DateTime> GenerateRandomTradingDates(int count, int daysBack)
        {
            var random = new Random();
            var randomDates = new HashSet<DateTime>();
            var startDate = DateTime.UtcNow.AddDays(-daysBack);

            while (randomDates.Count < count)
            {
                var randomDaysBack = random.Next(0, daysBack);
                var randomDate = DateTime.UtcNow.AddDays(-randomDaysBack).Date;

                // Only select weekdays (Monday-Friday)
                if (randomDate.DayOfWeek != DayOfWeek.Saturday &&
                    randomDate.DayOfWeek != DayOfWeek.Sunday &&
                    randomDate >= startDate)
                {
                    randomDates.Add(randomDate);
                }
            }

            return randomDates.OrderBy(d => d).ToList();
        }

        private async Task<bool> CheckTickerPriceChangedAsync(
            string ticker,
            List<DateTime> checkDates,
            ISsiServiceV2 ssiService,
            IOhlcvRepository ohlcvRepository,
            CancellationToken ct)
        {
            foreach (var checkDate in checkDates)
            {
                try
                {
                    // Get D1 from SSI V2 (adjusted price)
                    var request = new DailyOhlcRequest
                    {
                        Symbol = ticker,
                        Fromdate = checkDate.ToString("yyyy-MM-dd"),
                        Todate = checkDate.AddDays(1).ToString("yyyy-MM-dd"),
                        PageIndex = 1,
                        PageSize = 10
                    };
                    var (response, _) = await ssiService.FetchDailyOhlcAsync(request, ct);
                    var ssiData = response.Data?.Select(d => new Ohlcv
                    {
                        Ticker = ticker,
                        Timeframe = "D1",
                        Time = DateTime.Parse(d.TradingDate ?? string.Empty),
                        Open = decimal.Parse(d.Open ?? "0"),
                        High = decimal.Parse(d.High ?? "0"),
                        Low = decimal.Parse(d.Low ?? "0"),
                        Close = decimal.Parse(d.Close ?? "0"),
                        Volume = long.Parse(d.Volume ?? "0"),
                        CreatedAt = DateTime.UtcNow,
                        IsPreliminary = false
                    }).ToList() ?? new List<Ohlcv>();

                    // Get D1 from database
                    var dbData = await ohlcvRepository.GetByTickerAndTimeRangeAsync(
                        ticker,
                        "D1",
                        checkDate,
                        checkDate.AddDays(1),
                        ct);

                    if (ssiData.Any() && dbData.Any())
                    {
                        var ssiCandle = ssiData.First();
                        var dbCandle = dbData.First();

                        // Compare with 0.5% tolerance (corporate actions usually cause larger changes)
                        var priceDiffPercent = Math.Abs(dbCandle.Close - ssiCandle.Close) / ssiCandle.Close;

                        if (priceDiffPercent > 0.005m) // 0.5%
                        {
                            _logger.LogWarning(
                                "Price adjustment detected for {Ticker} on {Date}: " +
                                "DB Close={DbClose} vs SSI Close={SsiClose} (Diff: {Diff:P2})",
                                ticker, checkDate.ToString("yyyy-MM-dd"), dbCandle.Close, ssiCandle.Close, priceDiffPercent);

                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to check {Ticker} on {Date}. Continuing...",
                        ticker, checkDate.ToString("yyyy-MM-dd"));
                }
            }

            return false;
        }

        private async Task ReImportAllHistoricalDataAsync(
            string ticker,
            ISsiServiceV2 ssiService,
            IOhlcvRepository ohlcvRepository,
            IRedisService redisService,
            CancellationToken ct)
        {
            _logger.LogInformation("Starting re-import for {Ticker}...", ticker);

            // Get existing data range from database
            var oldestD1Date = await ohlcvRepository.GetOldestDateAsync(ticker, "D1", ct);
            var oldestM1Date = await ohlcvRepository.GetOldestDateAsync(ticker, "M1", ct);

            if (oldestD1Date == null && oldestM1Date == null)
            {
                _logger.LogWarning(
                    "No existing data for {Ticker}. Skipping re-import (nothing to adjust).",
                    ticker);
                return;
            }

            // === Re-import D1 ===
            if (oldestD1Date.HasValue)
            {
                await ReImportD1Async(ticker, oldestD1Date.Value, ssiService, ohlcvRepository, ct);
            }
            else
            {
                _logger.LogInformation("No D1 data exists for {Ticker}. Skipping D1 re-import.", ticker);
            }

            // === Re-import M1 (MANDATORY) ===
            if (oldestM1Date.HasValue)
            {
                await ReImportM1Async(ticker, oldestM1Date.Value, ssiService, ohlcvRepository, ct);
            }
            else
            {
                _logger.LogInformation("No M1 data exists for {Ticker}. Skipping M1 re-import.", ticker);
            }

            // === Invalidate Redis cache ===
            try
            {
                var cacheKeyPattern = RedisConstants.OhlcvPattern(ticker);
                await redisService.DeleteByPatternAsync(cacheKeyPattern);

                _logger.LogInformation("Invalidated cache for {Ticker}", ticker);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to invalidate cache for {Ticker}", ticker);
            }

            _logger.LogInformation("Completed re-import for {Ticker}", ticker);
        }

        private async Task ReImportD1Async(
            string ticker,
            DateTime oldestDate,
            ISsiServiceV2 ssiService,
            IOhlcvRepository ohlcvRepository,
            CancellationToken ct)
        {
            try
            {
                var d1StartDate = oldestDate;
                var d1EndDate = DateTime.UtcNow;

                _logger.LogInformation(
                    "Deleting old D1 data for {Ticker} from {Start} to {End} ({Days} days)...",
                    ticker, d1StartDate.ToString("yyyy-MM-dd"), d1EndDate.ToString("yyyy-MM-dd"),
                    (d1EndDate - d1StartDate).Days);

                await ohlcvRepository.DeleteByTickerAndTimeframeAsync(ticker, "D1", ct);

                _logger.LogInformation("Fetching adjusted D1 data from SSI V2 for {Ticker}...", ticker);

                var request = new DailyOhlcRequest
                {
                    Symbol = ticker,
                    Fromdate = d1StartDate.ToString("yyyy-MM-dd"),
                    Todate = d1EndDate.ToString("yyyy-MM-dd"),
                    PageIndex = 1,
                    PageSize = 10000
                };
                var (response, _) = await ssiService.FetchDailyOhlcAsync(request, ct);
                var d1Data = response.Data?.Select(d => new Ohlcv
                {
                    Ticker = ticker,
                    Timeframe = "D1",
                    Time = DateTime.Parse(d.TradingDate ?? string.Empty),
                    Open = decimal.Parse(d.Open ?? "0"),
                    High = decimal.Parse(d.High ?? "0"),
                    Low = decimal.Parse(d.Low ?? "0"),
                    Close = decimal.Parse(d.Close ?? "0"),
                    Volume = long.Parse(d.Volume ?? "0"),
                    CreatedAt = DateTime.UtcNow,
                    IsPreliminary = false
                }).ToList() ?? new List<Ohlcv>();

                if (d1Data.Any())
                {
                    _logger.LogInformation("Inserting {Count} D1 candles for {Ticker}...", d1Data.Count, ticker);
                    await ohlcvRepository.AddRangeAsync(d1Data, ct);
                    await ohlcvRepository.BulkUpsertAsync(d1Data, ct);

                    _logger.LogInformation("Re-imported {Count} D1 candles for {Ticker}", d1Data.Count, ticker);
                }
                else
                {
                    _logger.LogWarning("No D1 data returned from SSI for {Ticker}", ticker);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to re-import D1 data for {Ticker}", ticker);
                throw;
            }
        }

        private async Task ReImportM1Async(
            string ticker,
            DateTime oldestDate,
            ISsiServiceV2 ssiService,
            IOhlcvRepository ohlcvRepository,
            CancellationToken ct)
        {
            try
            {
                var m1StartDate = oldestDate;
                var m1EndDate = DateTime.UtcNow;

                _logger.LogInformation(
                    "Deleting old M1 data for {Ticker} from {Start} to {End}...",
                    ticker, m1StartDate.ToString("yyyy-MM-dd"), m1EndDate.ToString("yyyy-MM-dd"));

                await ohlcvRepository.DeleteByTickerAndTimeframeAsync(ticker, "M1", ct);

                // Import M1 by month to avoid timeout
                var currentDate = m1StartDate;
                var totalM1Imported = 0;
                var totalMonths = ((m1EndDate.Year - m1StartDate.Year) * 12) + m1EndDate.Month - m1StartDate.Month;

                _logger.LogInformation(
                    "Re-importing M1 for {Ticker}: {Months} months of data ({Days} days)...",
                    ticker, totalMonths, (m1EndDate - m1StartDate).Days);

                while (currentDate < m1EndDate)
                {
                    var monthEnd = currentDate.AddMonths(1);
                    if (monthEnd > m1EndDate) monthEnd = m1EndDate;

                    try
                    {
                        _logger.LogDebug(
                            "Fetching M1 for {Ticker}: {Start} to {End}...",
                            ticker, currentDate.ToString("yyyy-MM-dd"), monthEnd.ToString("yyyy-MM-dd"));

                        // Use SSI V2 API for intraday data
                        var intradayRequest = new IntradayOhlcRequest
                        {
                            Symbol = ticker,
                            FromDate = currentDate.ToString("yyyy-MM-dd"),
                            ToDate = monthEnd.ToString("yyyy-MM-dd"),
                            PageIndex = 1,
                            PageSize = 10000,
                            Ascending = true,
                            Resolution = 1 // 1 minute
                        };
                        var (intradayResponse, _) = await ssiService.FetchIntradayOhlcAsync(intradayRequest, ct);
                        var m1Data = intradayResponse.Data?.Select(d => new Ohlcv
                        {
                            Ticker = ticker,
                            Timeframe = "M1",
                            Time = DateTime.Parse(d.TradingDate ?? string.Empty),
                            Open = decimal.Parse(d.Open ?? "0"),
                            High = decimal.Parse(d.High ?? "0"),
                            Low = decimal.Parse(d.Low ?? "0"),
                            Close = decimal.Parse(d.Close ?? "0"),
                            Volume = long.Parse(d.Volume ?? "0"),
                            CreatedAt = DateTime.UtcNow,
                            IsPreliminary = false
                        }).ToList() ?? new List<Ohlcv>();

                        if (m1Data.Any())
                        {
                            await ohlcvRepository.AddRangeAsync(m1Data, ct);
                            await ohlcvRepository.BulkUpsertAsync(m1Data, ct);
                            totalM1Imported += m1Data.Count;

                            _logger.LogInformation(
                                "Imported {Count} M1 candles for {Ticker} ({Month}) - Total: {Total}",
                                m1Data.Count, ticker, currentDate.ToString("yyyy-MM"), totalM1Imported);
                        }

                        // Delay to avoid SSI API rate limiting
                        await Task.Delay(500, ct);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "Failed to import M1 for {Ticker} in {Month}. Continuing...",
                            ticker, currentDate.ToString("yyyy-MM"));
                    }

                    currentDate = monthEnd;
                }

                _logger.LogInformation(
                    "Re-imported total {Count} M1 candles for {Ticker}",
                    totalM1Imported, ticker);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to re-import M1 data for {Ticker}", ticker);
                throw;
            }
        }
    }
}
