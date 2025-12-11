using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Constants.SSI;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportSymbolsFromSsiV1
{
    public class ImportSymbolsFromSsiCommandHandlerV1(
        ISsiServiceV1 ssiService,
        IUnitOfWork uow,
        ILogger<ImportSymbolsFromSsiCommandHandlerV1> logger) : IRequestHandler<ImportSymbolsFromSsiCommandV1, ImportSymbolsFromSsiV1Result>
    {
        private readonly ISsiServiceV1 _ssiService = ssiService;
        private readonly IUnitOfWork _uow = uow;
        private readonly ILogger<ImportSymbolsFromSsiCommandHandlerV1> _logger = logger;

        /// <summary>
        /// Handles the import of symbol data from SSI API for all exchanges (HSX, HNX, UPCOM).
        /// Fetches symbols from each exchange, compares with existing database records,
        /// and performs bulk insert/update operations.
        /// </summary>
        /// <param name="request">The import command request.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        public async Task<ImportSymbolsFromSsiV1Result> Handle(ImportSymbolsFromSsiCommandV1 request, CancellationToken cancellationToken)
        {
            try
            {
                (List<Symbol> HsxAddList, List<Symbol> HsxUpdateList) = await MapSsiSymbolsToDomainSymbolsAsync(SsiConstantsV1.SSI_EXCHANGE_HSX, cancellationToken);
                (List<Symbol> HnxAddList, List<Symbol> HnxUpdateList) = await MapSsiSymbolsToDomainSymbolsAsync(SsiConstantsV1.SSI_EXCHANGE_HNX, cancellationToken);
                (List<Symbol> UpComAddList, List<Symbol> upComUpdateList) = await MapSsiSymbolsToDomainSymbolsAsync(SsiConstantsV1.SSI_EXCHANGE_UPCOM, cancellationToken);

                List<Symbol> symbolToAdd = [.. HsxAddList, .. HnxAddList, .. UpComAddList];

                List<Symbol> symbolToUpdate = [.. HsxUpdateList, .. HnxUpdateList, .. upComUpdateList];

                await _uow.Symbols.AddRangeAsync(symbolToAdd, cancellationToken);
                _uow.Symbols.UpdateRange(symbolToUpdate);
                await _uow.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Successfully imported {Count} new symbols from SSI", symbolToAdd.Count);
                _logger.LogInformation("Successfully updated {Count} symbols from SSI", symbolToUpdate.Count);

                return new ImportSymbolsFromSsiV1Result(symbolToAdd.Count, symbolToUpdate.Count, $"Successfully imported {symbolToAdd.Count} symbols from SSI, successfully updated {symbolToUpdate.Count} symbols from SSI");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing symbols from SSI");
                return new ImportSymbolsFromSsiV1Result(0,0, "Error importing symbols from SSI");
            }
        }

        /// <summary>
        /// Maps SSI symbols to domain symbol entities for a given exchange.
        /// Compares fetched symbols with existing database records to determine which to add or update.
        /// </summary>
        /// <param name="ssiExchange">The SSI exchange code (e.g., hsx, hnx, upcom).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A tuple containing lists of symbols to add and symbols to update.</returns>
        private async Task<(List<Symbol> addList, List<Symbol> updateList)> MapSsiSymbolsToDomainSymbolsAsync(string ssiExchange, CancellationToken cancellationToken)
        {
            try
            {
                var domainExchange = MapSsiExchangeCodeToDomainCode(ssiExchange);

                IEnumerable<Symbol> dbSymbolsEnum = await GetExistedSymbols(ssiExchange, cancellationToken);

                var dbSymbolsDict = dbSymbolsEnum.ToDictionary(s => s.Ticker.ToUpper(), s => s);

                List<SsiSymbolDto> ssiSymbols = await _ssiService.FetchSymbolsListAsync(ssiExchange, cancellationToken);

                var validSsiSymbols = ssiSymbols
                    .Where(s => !string.IsNullOrEmpty(s.StockSymbol))
                    .DistinctBy(s => s.StockSymbol!.ToUpper());

                List<Symbol> addList = [];
                List<Symbol> updateList = [];

                foreach (var item in validSsiSymbols)
                {
                    var tickerKey = item.StockSymbol!.ToUpper();

                    var newType = MapSsiSymbolTypeToDomainType(item.StockType);
                    var newTradingStatus = MapSsiTradingStatusToDomainTradingStatus(item.TradingStatus);

                    if (dbSymbolsDict.TryGetValue(tickerKey, out var existingDbSymbol))
                    {
                        bool isChanged =
                            existingDbSymbol.Isin != item.Isin ||
                            existingDbSymbol.EnCompanyName != item.CompanyNameEn ||
                            existingDbSymbol.ViCompanyName != item.CompanyNameVi ||
                            existingDbSymbol.Type != newType ||
                            existingDbSymbol.TradingStatus != newTradingStatus;

                        if (isChanged)
                        {
                            existingDbSymbol.Isin = item.Isin;
                            existingDbSymbol.EnCompanyName = item.CompanyNameEn;
                            existingDbSymbol.ViCompanyName = item.CompanyNameVi;
                            existingDbSymbol.Type = newType;
                            existingDbSymbol.TradingStatus = newTradingStatus;

                            updateList.Add(existingDbSymbol);
                        }
                    }
                    else
                    {
                        addList.Add(new Symbol
                        {
                            Ticker = item.StockSymbol!,
                            Isin = item.Isin,
                            EnCompanyName = item.CompanyNameEn,
                            ViCompanyName = item.CompanyNameVi,
                            ExchangeCode = domainExchange,
                            Status = CommonStatus.Active,
                            Type = newType,
                            TradingStatus = newTradingStatus
                        });
                    }
                }

                return (addList, updateList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping SSI symbols for exchange {SsiExchange}", ssiExchange);
                return ([], []);
            }
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
                SsiConstantsV1.SSI_SYMBOL_TYPE_STOCK => SymbolType.Stock,
                SsiConstantsV1.SSI_SYMBOL_TYPE_ETF => SymbolType.ETF,
                SsiConstantsV1.SSI_SYMBOL_TYPE_BOND => SymbolType.BOND,
                SsiConstantsV1.SSI_SYMBOL_TYPE_MUTUAL_FUND => SymbolType.MutualFund,
                _ => SymbolType.Unknown,
            };
        }

        /// <summary>
        /// Maps SSI trading status string to domain <see cref="SymbolStatus"/> enum.
        /// </summary>
        /// <param name="ssiStatus">The trading status string from SSI API.</param>
        /// <returns>The corresponding <see cref="SymbolStatus"/> enum value, or Unknown if not matched.</returns>
        private static SymbolStatus MapSsiTradingStatusToDomainTradingStatus(string? ssiStatus)
        {
            return ssiStatus switch
            {
                SsiConstantsV1.SSI_SYMBOL_TRADING_STATUS_NORMAL => SymbolStatus.Normal,
                SsiConstantsV1.SSI_SYMBOL_TRADING_STATUS_DELISTED => SymbolStatus.Delisted,
                SsiConstantsV1.SSI_SYMBOL_TRADING_STATUS_HALT => SymbolStatus.Halt,
                SsiConstantsV1.SSI_SYMBOL_TRADING_STATUS_SUSPEND => SymbolStatus.Suspend,
                SsiConstantsV1.SSI_SYMBOL_TRADING_STATUS_NEW_LIST => SymbolStatus.NewList,
                SsiConstantsV1.SSI_SYMBOL_TRADING_STATUS_NEAR_DELIST => SymbolStatus.NearDelist,
                SsiConstantsV1.SSI_SYMBOL_TRADING_STATUS_SPECIAL_TRADING => SymbolStatus.SpecialTrading,
                SsiConstantsV1.SSI_SYMBOL_TRADING_STATUS_SUSPENDED_A => SymbolStatus.SuspendA,
                SsiConstantsV1.SSI_SYMBOL_TRADING_STATUS_SUSPENDED_PT => SymbolStatus.SuspendPT,
                _ => SymbolStatus.Unknown,
            };
        }

        /// <summary>
        /// Maps SSI exchange code to domain exchange constant.
        /// </summary>
        /// <param name="ssiExchangeCode">The SSI exchange code (e.g., "hsx", "hnx", "upcom").</param>
        /// <returns>The corresponding domain exchange constant, or "UNKNOWN" if not matched.</returns>
        private static string MapSsiExchangeCodeToDomainCode(string ssiExchangeCode)
        {
            return ssiExchangeCode.ToLower() switch
            {
                SsiConstantsV1.SSI_EXCHANGE_HSX => ExchangeConstant.EXCHANGE_HSX,
                SsiConstantsV1.SSI_EXCHANGE_HNX => ExchangeConstant.EXCHANGE_HNX,
                SsiConstantsV1.SSI_EXCHANGE_UPCOM => ExchangeConstant.EXCHANGE_UPCOM,
                _ => "UNKNOWN",
            };
        }

        /// <summary>
        /// Retrieves existing symbols from the database for a specific exchange.
        /// </summary>
        /// <param name="exchange">The SSI exchange code to filter symbols by.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>An enumerable of existing <see cref="Symbol"/> entities for the specified exchange.</returns>
        private async Task<IEnumerable<Symbol>> GetExistedSymbols(string exchange, CancellationToken cancellationToken)
        {
            return await _uow.Symbols.FindAsync(
                s => string.Equals(s.ExchangeCode.ToUpper(), MapSsiExchangeCodeToDomainCode(exchange).ToUpper()),
                cancellationToken);
        }
    }
}
