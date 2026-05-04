using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportIndexConstituentsFromSsi
{
    /// <summary>
    /// Handles <see cref="ImportIndexConstituentsFromSsiCommand"/>.
    /// For every active <see cref="MarketIndex"/> in the database it calls the SSI
    /// <c>/stock/group/{indexCode}</c> endpoint, maps the returned symbols to
    /// <see cref="MarketIndexSymbol"/> records, and bulk-upserts them.
    /// </summary>
    public class ImportIndexConstituentsFromSsiCommandHandler(
        ISsiServiceV1 ssiService,
        IUnitOfWork uow,
        ILogger<ImportIndexConstituentsFromSsiCommandHandler> logger)
        : IRequestHandler<ImportIndexConstituentsFromSsiCommand, ImportIndexConstituentsFromSsiResult>
    {
        private readonly ISsiServiceV1 _ssiService = ssiService;
        private readonly IUnitOfWork _uow = uow;
        private readonly ILogger<ImportIndexConstituentsFromSsiCommandHandler> _logger = logger;

        public async Task<ImportIndexConstituentsFromSsiResult> Handle(
            ImportIndexConstituentsFromSsiCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // 1. Get all active market indices from the database
                var allIndices = _uow.MarketIndices
                    .GetQueryable()
                    .Where(i => i.Status == CommonStatus.Active)
                    .ToList();

                if (allIndices.Count == 0)
                {
                    _logger.LogWarning("No active market indices found in database. Skipping import.");
                    return new ImportIndexConstituentsFromSsiResult(0, 0, "No active market indices found.");
                }

                _logger.LogInformation("Found {Count} active market indices. Starting constituent import.", allIndices.Count);

                int totalUpserted = 0;
                int processedIndices = 0;

                foreach (var index in allIndices)
                {
                    try
                    {
                        // 2. Fetch constituents from SSI for this index
                        var ssiSymbols = await _ssiService.FetchIndexConstituentsAsync(index.Code, cancellationToken);

                        if (ssiSymbols.Count == 0)
                        {
                            _logger.LogWarning("SSI returned no constituents for index {IndexCode}. Skipping.", index.Code);
                            continue;
                        }

                        // 3. Get tickers that exist in our symbols table so we don't create orphan FK rows
                        var returnedTickers = ssiSymbols
                            .Where(s => !string.IsNullOrWhiteSpace(s.StockSymbol))
                            .Select(s => s.StockSymbol!.ToUpper())
                            .Distinct()
                            .ToList();

                        var existingSymbols = await _uow.Symbols.FindAsync(
                            s => returnedTickers.Contains(s.Ticker), cancellationToken);
                        var existingTickers = existingSymbols.Select(s => s.Ticker).ToHashSet();

                        // 4. Get currently active constituents in DB so we can mark removed ones inactive
                        var currentActiveTickers = _uow.MarketIndexSymbols
                            .GetQueryable()
                            .Where(m => m.IndexCode == index.Code && m.IsActive)
                            .Select(m => m.Ticker)
                            .ToHashSet();

                        var today = DateOnly.FromDateTime(DateTime.UtcNow);

                        // 5. Build upsert list from SSI data
                        var constituents = new List<MarketIndexSymbol>();
                        int displayOrder = 1;

                        foreach (var ssiSymbol in ssiSymbols
                            .Where(s => !string.IsNullOrWhiteSpace(s.StockSymbol))
                            .DistinctBy(s => s.StockSymbol!.ToUpper()))
                        {
                            var ticker = ssiSymbol.StockSymbol!.ToUpper();

                            // Skip symbols not in our symbols table (avoids FK violation)
                            if (!existingTickers.Contains(ticker))
                            {
                                _logger.LogDebug("Ticker {Ticker} from index {Index} not found in symbols table — skipping.", ticker, index.Code);
                                continue;
                            }

                            constituents.Add(new MarketIndexSymbol
                            {
                                IndexCode = index.Code,
                                Ticker = ticker,
                                IsActive = true,
                                AddedDate = today,
                                RemovedDate = null,
                                DisplayOrder = displayOrder++,
                            });
                        }

                        // 6. Mark previously-active constituents that are no longer in the SSI response as inactive
                        var newTickerSet = constituents.Select(c => c.Ticker).ToHashSet();
                        var removedTickers = currentActiveTickers.Except(newTickerSet).ToList();

                        foreach (var removed in removedTickers)
                        {
                            constituents.Add(new MarketIndexSymbol
                            {
                                IndexCode = index.Code,
                                Ticker = removed,
                                IsActive = false,
                                RemovedDate = today,
                                DisplayOrder = 0,
                            });
                        }

                        // 7. Bulk upsert
                        await _uow.MarketIndexSymbols.BulkUpsertAsync(constituents, cancellationToken);
                        await _uow.SaveChangesAsync(cancellationToken);

                        totalUpserted += constituents.Count;
                        processedIndices++;

                        _logger.LogInformation(
                            "Index {Code}: upserted {Active} active, {Removed} removed constituents.",
                            index.Code, constituents.Count(c => c.IsActive), removedTickers.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to import constituents for index {IndexCode}. Continuing with next index.", index.Code);
                    }
                }

                var message = $"Processed {processedIndices}/{allIndices.Count} indices, upserted {totalUpserted} constituent records.";
                _logger.LogInformation(message);
                return new ImportIndexConstituentsFromSsiResult(processedIndices, totalUpserted, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing index constituents from SSI.");
                return new ImportIndexConstituentsFromSsiResult(0, 0, "Error importing index constituents from SSI.");
            }
        }
    }
}
