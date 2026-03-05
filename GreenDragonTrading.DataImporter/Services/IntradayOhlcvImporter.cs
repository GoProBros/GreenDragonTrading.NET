using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace GreenDragonTrading.DataImporter.Services;

public class IntradayOhlcvImporter
{
    private readonly ISsiServiceV2 _ssiService;
    private readonly IOhlcvUnitOfWork _ohlcvUow;
    private readonly ILogger<IntradayOhlcvImporter> _logger;
    private readonly ImportSettings _settings;

    public IntradayOhlcvImporter(
        ISsiServiceV2 ssiService,
        IOhlcvUnitOfWork ohlcvUow,
        ILogger<IntradayOhlcvImporter> logger,
        ImportSettings settings)
    {
        _ssiService = ssiService;
        _ohlcvUow = ohlcvUow;
        _logger = logger;
        _settings = settings;
    }

    public async Task<ImportResult> ImportAsync(string ticker, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var result = new ImportResult { Ticker = ticker };
        
        try
        {
            _logger.LogInformation("Importing M1 data for {Ticker} from {FromDate:yyyy-MM-dd} to {ToDate:yyyy-MM-dd}", 
                ticker, fromDate, toDate);

            // SSI API chỉ cho phép lấy tối đa 30 ngày mỗi request
            // Chia thành các batch 30 ngày
            var batches = SplitDateRangeIntoBatches(fromDate, toDate, 30);
            _logger.LogInformation("Split date range into {Count} batches (30 days each)", batches.Count);

            var allEntities = new List<Domain.Entities.Ohlcv>();

            foreach (var (batchFrom, batchTo) in batches)
            {
                _logger.LogInformation("Fetching batch: {From:yyyy-MM-dd} to {To:yyyy-MM-dd}", batchFrom, batchTo);

                var request = new IntradayOhlcRequest
                {
                    Symbol = ticker,
                    FromDate = batchFrom.ToString("dd/MM/yyyy"),
                    ToDate = batchTo.ToString("dd/MM/yyyy"),
                    PageIndex = 1,
                    PageSize = 5000,
                    Ascending = true,
                    Resolution = 1 // 1 minute
                };

                // Retry logic with exponential backoff
                var maxRetries = 3;
                var retryCount = 0;
                IntradayOhlcResponse? ssiResponse = null;
                int recordCount = 0;
                bool batchFailed = false;

                while (retryCount < maxRetries)
                {
                    try
                    {
                        (ssiResponse, recordCount) = await _ssiService.FetchIntradayOhlcAsync(request, cancellationToken);
                        _logger.LogInformation("Received {Count} records from batch", recordCount);
                        break; // Success, exit retry loop
                    }
                    catch (TaskCanceledException ex)
                    {
                        retryCount++;
                        if (retryCount >= maxRetries)
                        {
                            _logger.LogError(ex, "Failed to fetch batch after {Retries} retries: {From} to {To} - Skipping batch", 
                                maxRetries, batchFrom.ToString("yyyy-MM-dd"), batchTo.ToString("yyyy-MM-dd"));
                            result.FailedCount++;
                            batchFailed = true;
                            break; // Exit retry loop, skip this batch
                        }

                        var delayMs = 1000 * (int)Math.Pow(2, retryCount); // 2s, 4s, 8s
                        _logger.LogWarning("Request timeout, retrying in {Delay}ms (attempt {Retry}/{Max})", 
                            delayMs, retryCount + 1, maxRetries);
                        await Task.Delay(delayMs, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error fetching batch: {From} to {To} - Skipping batch", 
                            batchFrom.ToString("yyyy-MM-dd"), batchTo.ToString("yyyy-MM-dd"));
                        result.FailedCount++;
                        batchFailed = true;
                        break; // Exit retry loop, skip this batch
                    }
                }

                // Skip processing if batch failed
                if (batchFailed)
                {
                    continue;
                }

                if (ssiResponse?.Data != null && ssiResponse.Data.Count > 0)
                {
                    foreach (var intradayData in ssiResponse.Data)
                    {
                        try
                        {
                            var entity = ConvertToEntity(intradayData);
                            allEntities.Add(entity);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to convert data for {Ticker} on {Date} {Time}", 
                                ticker, intradayData.TradingDate, intradayData.Time);
                            result.FailedCount++;
                        }
                    }
                }

                // Delay giữa các batch để tránh rate limit
                if (batches.IndexOf((batchFrom, batchTo)) < batches.Count - 1)
                {
                    await Task.Delay(500, cancellationToken);
                }
            }

            if (allEntities.Count > 0)
            {
                var inserted = await _ohlcvUow.Ohlcv.BulkUpsertAsync(allEntities, cancellationToken);
                result.SuccessCount = inserted;
                _logger.LogInformation("Upserted {Inserted}/{Total} M1 records for {Ticker} ({Skipped} duplicates skipped)", 
                    inserted, allEntities.Count, ticker, allEntities.Count - inserted);
            }
            else
            {
                _logger.LogInformation("No new records to import for {Ticker}", ticker);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing M1 data for {Ticker}", ticker);
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    private List<(DateTime from, DateTime to)> SplitDateRangeIntoBatches(DateTime fromDate, DateTime toDate, int daysPerBatch)
    {
        var batches = new List<(DateTime, DateTime)>();
        var currentFrom = fromDate;

        while (currentFrom < toDate)
        {
            var currentTo = currentFrom.AddDays(daysPerBatch);
            if (currentTo > toDate)
                currentTo = toDate;

            batches.Add((currentFrom, currentTo));
            currentFrom = currentTo.AddDays(1);
        }

        return batches;
    }

    public async Task<ImportBatchResult> ImportBatchAsync(
        List<string> tickers, 
        DateTime fromDate, 
        DateTime toDate,
        Models.ImportProgress? progress = null,
        CancellationToken cancellationToken = default)
    {
        var batchResult = new ImportBatchResult();
        var totalBatches = (int)Math.Ceiling((double)tickers.Count / _settings.BatchSize);

        _logger.LogInformation("Starting batch import for {Count} symbols in {Batches} batches", 
            tickers.Count, totalBatches);

        for (int i = 0; i < tickers.Count; i += _settings.BatchSize)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Import cancelled by user");
                break;
            }

            var batch = tickers.Skip(i).Take(_settings.BatchSize).ToList();
            var batchNumber = (i / _settings.BatchSize) + 1;

            _logger.LogInformation("Processing batch {Current}/{Total} ({Count} symbols)", 
                batchNumber, totalBatches, batch.Count);

            foreach (var ticker in batch)
            {
                var result = await ImportWithRetryAsync(ticker, fromDate, toDate, cancellationToken);
                batchResult.Results.Add(result);
                
                if (result.SuccessCount > 0)
                    batchResult.TotalSuccess += result.SuccessCount;
                if (result.FailedCount > 0)
                    batchResult.TotalFailed += result.FailedCount;
                    
                // Update progress
                if (progress != null)
                {
                    progress.ProcessedSymbols++;
                    
                    if (result.SuccessCount > 0)
                    {
                        progress.SuccessSymbols++;
                        progress.CompletedTickers.Add(ticker);
                    }
                    else if (!string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        progress.FailedSymbols++;
                        progress.FailedTickers.Add(ticker);
                    }
                    else
                    {
                        // Xử lý thành công nhưng không có data mới (all duplicates hoặc no data từ SSI)
                        progress.NoDataSymbols++;
                        progress.NoDataTickers.Add(ticker);
                    }
                    
                    progress.RemainingTickers.Remove(ticker);
                    progress.Save();
                }

                // Respect SSI rate limit (1 req/s)
                if (!cancellationToken.IsCancellationRequested)
                    await Task.Delay(_settings.DelayBetweenSymbolsMs, cancellationToken);
            }

            // Delay between batches
            if (i + _settings.BatchSize < tickers.Count)
            {
                _logger.LogInformation("Waiting {Delay}ms before next batch...", _settings.DelayBetweenBatchesMs);
                await Task.Delay(_settings.DelayBetweenBatchesMs, cancellationToken);
            }
        }

        _logger.LogInformation("Batch import completed. Success: {Success}, Failed: {Failed}", 
            batchResult.TotalSuccess, batchResult.TotalFailed);

        return batchResult;
    }

    private async Task<ImportResult> ImportWithRetryAsync(
        string ticker, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken)
    {
        for (int attempt = 1; attempt <= _settings.MaxRetries; attempt++)
        {
            try
            {
                return await ImportAsync(ticker, fromDate, toDate, cancellationToken);
            }
            catch (Exception ex)
            {
                // If the user cancelled, propagate immediately — do not retry
                if (cancellationToken.IsCancellationRequested)
                    throw;

                if (attempt == _settings.MaxRetries)
                {
                    _logger.LogError(ex, "Failed to import {Ticker} after {Attempts} attempts", 
                        ticker, _settings.MaxRetries);
                    return new ImportResult 
                    { 
                        Ticker = ticker, 
                        ErrorMessage = $"Failed after {_settings.MaxRetries} attempts: {ex.Message}" 
                    };
                }

                _logger.LogWarning("Attempt {Attempt}/{Max} failed for {Ticker}, retrying in {Delay}ms...", 
                    attempt, _settings.MaxRetries, ticker, _settings.RetryDelayMs);
                
                await Task.Delay(_settings.RetryDelayMs, cancellationToken);
            }
        }

        return new ImportResult { Ticker = ticker, ErrorMessage = "Unexpected error" };
    }

    // SSI returns timestamps in Vietnam time (UTC+7). Use this zone for conversion.
    private static readonly TimeZoneInfo _vnZone =
        TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

    private Domain.Entities.Ohlcv ConvertToEntity(IntradayOhlcResponseModel data)
    {
        // Parse TradingDate (dd/MM/yyyy) and Time (HH:mm:ss) — both in Vietnam time (UTC+7)
        var tradingDate = DateTime.ParseExact(data.TradingDate!, "dd/MM/yyyy", CultureInfo.InvariantCulture);
        var time = TimeSpan.ParseExact(data.Time!, @"hh\:mm\:ss", CultureInfo.InvariantCulture);
        // Combine into a Vietnam-local DateTime, then convert to UTC for storage.
        var vnDateTime = DateTime.SpecifyKind(tradingDate.Add(time), DateTimeKind.Unspecified);
        
        return new Domain.Entities.Ohlcv
        {
            Time = TimeZoneInfo.ConvertTimeToUtc(vnDateTime, _vnZone),
            Ticker = data.Symbol!.ToUpper(),
            Timeframe = OhlcvConstants.Timeframes.M1,
            Open = decimal.Parse(data.Open!),
            High = decimal.Parse(data.High!),
            Low = decimal.Parse(data.Low!),
            Close = decimal.Parse(data.Close!),
            Volume = long.Parse(data.Volume!),
            Value = decimal.Parse(data.Value!),
            TradesCount = 0,
            CreatedAt = DateTime.UtcNow,
            Source = OhlcvConstants.Sources.SsiApi
        };
    }
}
