using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace GreenDragonTrading.DataImporter.Services;

public class DailyOhlcvImporter
{
    private readonly ISsiServiceV2 _ssiService;
    private readonly IOhlcvUnitOfWork _ohlcvUow;
    private readonly ILogger<DailyOhlcvImporter> _logger;
    private readonly ImportSettings _settings;

    public DailyOhlcvImporter(
        ISsiServiceV2 ssiService,
        IOhlcvUnitOfWork ohlcvUow,
        ILogger<DailyOhlcvImporter> logger,
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
            _logger.LogInformation("Importing D1 data for {Ticker} from {FromDate:yyyy-MM-dd} to {ToDate:yyyy-MM-dd}", 
                ticker, fromDate, toDate);

            var request = new DailyOhlcRequest
            {
                Symbol = ticker,
                Fromdate = fromDate.ToString("dd/MM/yyyy"),
                Todate = toDate.ToString("dd/MM/yyyy"),
                PageIndex = 1,
                PageSize = 5000 
            };

            var (ssiResponse, recordCount) = await _ssiService.FetchDailyOhlcAsync(request, cancellationToken);

            _logger.LogInformation("Received {Count} records from SSI API", recordCount);

            if (ssiResponse?.Data == null || ssiResponse.Data.Count == 0)
            {
                _logger.LogWarning("No data returned for {Ticker}", ticker);
                return result;
            }

            var entities = new List<Domain.Entities.Ohlcv>();
            
            foreach (var dailyData in ssiResponse.Data)
            {
                try
                {
                    var entity = ConvertToEntity(dailyData);
                    entities.Add(entity);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to convert data for {Ticker} on {Date}", 
                        ticker, dailyData.TradingDate);
                    result.FailedCount++;
                }
            }

            if (entities.Count > 0)
            {
                var inserted = await _ohlcvUow.Ohlcv.BulkUpsertAsync(entities, cancellationToken);
                result.SuccessCount = inserted;
                _logger.LogInformation("Upserted {Inserted}/{Total} D1 records for {Ticker} ({Skipped} duplicates skipped)", 
                    inserted, entities.Count, ticker, entities.Count - inserted);
            }
            else
            {
                _logger.LogInformation("No new records to import for {Ticker}", ticker);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing D1 data for {Ticker}", ticker);
            result.ErrorMessage = ex.Message;
        }

        return result;
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

    private Domain.Entities.Ohlcv ConvertToEntity(DailyOhlcResponseModel data)
    {
        var tradingDate = DateTime.ParseExact(data.TradingDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
        
        return new Domain.Entities.Ohlcv
        {
            Time = DateTime.SpecifyKind(tradingDate, DateTimeKind.Utc),
            Ticker = data.Symbol.ToUpper(),
            Timeframe = OhlcvConstants.Timeframes.D1,
            Open = decimal.Parse(data.Open),
            High = decimal.Parse(data.High),
            Low = decimal.Parse(data.Low),
            Close = decimal.Parse(data.Close),
            Volume = long.Parse(data.Volume),
            Value = decimal.Parse(data.Value),
            TradesCount = 0,
            CreatedAt = DateTime.UtcNow,
            Source = OhlcvConstants.Sources.SsiApi
        };
    }
}

public class ImportResult
{
    public string Ticker { get; set; } = string.Empty;
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ImportBatchResult
{
    public List<ImportResult> Results { get; set; } = new();
    public int TotalSuccess { get; set; }
    public int TotalFailed { get; set; }
}
