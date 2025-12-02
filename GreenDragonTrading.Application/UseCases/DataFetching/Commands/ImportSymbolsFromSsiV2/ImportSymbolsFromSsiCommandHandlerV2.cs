using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportSymbolsFromSsiV1;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Constants.SSI;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportSymbolsFromSsiV2
{
    public class ImportSymbolsFromSsiCommandHandlerV2(
        ISsiServiceV2 ssiSerive,
        ILogger<ImportSymbolsFromSsiCommandHandlerV2> logger,
        IUnitOfWork uow) : IRequestHandler<ImportSymbolsFromSsiCommandV2, ImportSymbolsFromSsiV2Result>
    {
        private readonly ISsiServiceV2 _ssiService = ssiSerive;
        private readonly ILogger<ImportSymbolsFromSsiCommandHandlerV2> _logger = logger;
        private readonly IUnitOfWork _uow = uow;
        private readonly List<string> availableType = [SsiConstantsV2.SSI_SYMBOL_TYPE_ETF, SsiConstantsV2.SSI_SYMBOL_TYPE_STOCK];
        private readonly List<string> availableExchange = [SsiConstantsV2.SSI_EXCHANGE_HNX, SsiConstantsV2.SSI_EXCHANGE_HSX, SsiConstantsV2.SSI_EXCHANGE_UPCOM];

        /// <summary>
        /// Handles the import of symbol data from SSI API for all exchanges (HSX, HNX, UPCOM).
        /// Fetches symbols from each exchange, compares with existing database records,
        /// and performs bulk insert/update operations.
        /// </summary>
        /// <param name="request">The import command request.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A <see cref="ImportSymbolsFromSsiResult"/> containing counts of imported and updated symbols.</returns>
        public async Task<ImportSymbolsFromSsiV2Result> Handle(ImportSymbolsFromSsiCommandV2 request, CancellationToken cancellationToken)
        {
            try
            {
                (List<Symbol> symbolsToAdd, List<Symbol> symbolsToUpdate) = await MapSsiSymbolsToDomainSymbolsAsync(cancellationToken);

                await _uow.Symbols.AddRangeAsync(symbolsToAdd, cancellationToken);
                _uow.Symbols.UpdateRange(symbolsToUpdate);
                await _uow.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Successfully imported {Count} new symbols from SSI", symbolsToAdd.Count);
                _logger.LogInformation("Successfully updated {Count} symbols from SSI", symbolsToUpdate.Count);

                return new ImportSymbolsFromSsiV2Result(symbolsToAdd.Count, symbolsToUpdate.Count, $"Successfully imported {symbolsToAdd.Count} symbols from SSI, successfully updated {symbolsToUpdate.Count} symbols from SSI");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing symbols from SSI");
                return new ImportSymbolsFromSsiV2Result(0, 0, "Error importing symbols from SSI");
            }
        }

        /// <summary>
        /// Maps SSI symbols to domain symbol entities for a given exchange.
        /// Compares fetched symbols with existing database records to determine which to add or update.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A tuple containing lists of symbols to add and symbols to update.</returns>
        private async Task<(List<Symbol> addList, List<Symbol> updateList)> MapSsiSymbolsToDomainSymbolsAsync(CancellationToken cancellationToken)
        {
            try
            {
                IEnumerable<Symbol> dbSymbolsEnum = await GetExistedSymbols(cancellationToken);

                var dbSymbolsDict = dbSymbolsEnum.ToDictionary(s => s.Ticker.ToUpper(), s => s);

                SecuritiesDetailsRequest requestQuery = new()
                {
                    Market = null,
                    Symbol = null,
                    PageIndex = 1,
                    PageSize = 1000
                };

                int queriedCount = 0;
                int total = 0;
                List<RepeatedSecuritiesDetailsInfo>? validSsiSymbols = [];
                do
                {
                    (SecuritiesDetailsResponse ssiSymbols, int count) = await _ssiService.FetchSecuritiesDetails(requestQuery, cancellationToken);

                    var securities = ssiSymbols.Data
                        .Where(d => d.RepeatedInfo != null)
                        .SelectMany(d => d.RepeatedInfo);

                    var batchSymbols = securities
                        .Where(s => !string.IsNullOrEmpty(s.Symbol))
                        .Where(s => !string.IsNullOrEmpty(s.SecType) && availableType.Contains(s.SecType!))
                        .Where(s => !string.IsNullOrEmpty(s.Exchange) && availableExchange.Contains(s.Exchange!))
                        .DistinctBy(s => s.Symbol?.ToUpper())
                        .ToList();

                    validSsiSymbols.AddRange(batchSymbols);

                    if (total == 0)
                    {
                        string? stringTotal = ssiSymbols.Data
                            .FirstOrDefault(d => d.TotalNoSym != null)?.TotalNoSym;

                        _ = int.TryParse(stringTotal, out total);
                    }

                    queriedCount += count;
                    requestQuery.PageIndex++;
                }
                while (queriedCount < total);

                List<Symbol> addList = [];
                List<Symbol> updateList = [];

                foreach (var item in validSsiSymbols)
                {
                    var tickerKey = item.Symbol!.ToUpper();

                    var newType = MapSsiSymbolTypeToDomainType(item.SecType);
                    var newExchange = MapSsiExchangeCodeToDomainCode(item.Exchange);

                    if (dbSymbolsDict.TryGetValue(tickerKey, out var existingDbSymbol))
                    {
                        bool isChanged =
                            existingDbSymbol.Isin != item.Isin ||
                            existingDbSymbol.EnCompanyName != item.SymbolEngName ||
                            existingDbSymbol.ViCompanyName != item.SymbolName ||
                            existingDbSymbol.Type != newType;

                        if (isChanged)
                        {
                            existingDbSymbol.Isin = item.Isin;
                            existingDbSymbol.EnCompanyName = item.SymbolEngName;
                            existingDbSymbol.ViCompanyName = item.SymbolName;
                            existingDbSymbol.Type = newType;

                            updateList.Add(existingDbSymbol);
                        }
                    }
                    else
                    {
                        addList.Add(new Symbol
                        {
                            Ticker = item.Symbol!,
                            Isin = item.Isin,
                            EnCompanyName = item.SymbolEngName,
                            ViCompanyName = item.SymbolName,
                            ExchangeCode = newExchange,
                            Status = CommonStatus.Active,
                            Type = newType,
                            TradingStatus = SymbolStatus.Normal
                        });
                    }
                }

                return (addList, updateList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping SSI symbols.");
                return ([], []);
            }
        }

        /// <summary>
        /// Retrieves existing symbols from the database for a specific exchange.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>An enumerable of existing <see cref="Symbol"/> entities for the specified exchange.</returns>
        private async Task<IEnumerable<Symbol>> GetExistedSymbols(CancellationToken cancellationToken)
        {
            return await _uow.Symbols.GetAllAsync(cancellationToken);
        }

        /// <summary>
        /// Maps SSI symbol type string to domain <see cref="SymbolType"/> enum.
        /// </summary>
        /// <param name="ssiType">The symbol type string from SSI API (e.g., "s", "e", "b", "m").</param>
        /// <returns>The corresponding <see cref="SymbolType"/> enum value, or Unknown if not matched.</returns>
        private static SymbolType MapSsiSymbolTypeToDomainType(string? ssiType)
        {
            return ssiType switch
            {
                SsiConstantsV2.SSI_SYMBOL_TYPE_STOCK => SymbolType.Stock,
                SsiConstantsV2.SSI_SYMBOL_TYPE_ETF => SymbolType.ETF,
                SsiConstantsV2.SSI_SYMBOL_TYPE_BOND => SymbolType.BOND,
                SsiConstantsV2.SSI_SYMBOL_TYPE_MUTUAL_FUND => SymbolType.MutualFund,
                SsiConstantsV2.SSI_SYMBOL_TYPE_COVERED_WARRANT => SymbolType.CoveredWarrant,
                SsiConstantsV2.SSI_SYMBOL_TYPE_FUTURES => SymbolType.Futures,
                _ => SymbolType.Unknown,
            };
        }

        /// <summary>
        /// Maps SSI exchange code to domain exchange constant.
        /// </summary>
        /// <param name="ssiExchangeCode">The SSI exchange code (e.g., "hsx", "hnx", "upcom").</param>
        /// <returns>The corresponding domain exchange constant, or "UNKNOWN" if not matched.</returns>
        private static string MapSsiExchangeCodeToDomainCode(string? ssiExchangeCode)
        {
            return ssiExchangeCode switch
            {
                SsiConstantsV2.SSI_EXCHANGE_HSX => ExchangeConstant.EXCHANGE_HSX,
                SsiConstantsV2.SSI_EXCHANGE_HNX => ExchangeConstant.EXCHANGE_HNX,
                SsiConstantsV2.SSI_EXCHANGE_UPCOM => ExchangeConstant.EXCHANGE_UPCOM,
                _ => "UNKNOWN",
            };
        }
    }
}
