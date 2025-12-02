using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportSymbolsFromSsi
{
    public class ImportSymbolsFromSsiCommandHandler(
        ISsiService ssiService,
        IUnitOfWork uow,
        ILogger<ImportSymbolsFromSsiCommandHandler> logger) : IRequestHandler<ImportSymbolsFromSsiCommand, ImportSymbolsFromSsiResult>
    {
        private readonly ISsiService _ssiService = ssiService;
        private readonly IUnitOfWork _uow = uow;
        private readonly ILogger<ImportSymbolsFromSsiCommandHandler> _logger = logger;

        public async Task<ImportSymbolsFromSsiResult> Handle(ImportSymbolsFromSsiCommand request, CancellationToken cancellationToken)
        {
            try
            {
                List<Symbol> symbolToAdd = [
                    .. await MapSsiSymbolsToDomainSymbols(SymbolConstant.SSI_EXCHANGE_HSX, cancellationToken),
                    .. await MapSsiSymbolsToDomainSymbols(SymbolConstant.SSI_EXCHANGE_HNX, cancellationToken),
                    .. await MapSsiSymbolsToDomainSymbols(SymbolConstant.SSI_EXCHANGE_UPCOM, cancellationToken)
                ];

                if (symbolToAdd.Count <= 0) return new ImportSymbolsFromSsiResult(0, "No new symbols to import from SSI");

                await _uow.Symbols.AddRangeAsync(symbolToAdd, cancellationToken);
                await _uow.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Successfully imported {Count} symbols from SSI", symbolToAdd.Count);

                return new ImportSymbolsFromSsiResult(symbolToAdd.Count, $"Successfully imported {symbolToAdd.Count} symbols from SSI");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing symbols from SSI");
                return new ImportSymbolsFromSsiResult(0, "Error importing symbols from SSI");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ssiType"></param>
        /// <returns></returns>
        private static SymbolType MapSsiSymbolTypeToDomainType(string? ssiType)
        {
            return ssiType switch
            {
                SymbolConstant.SSI_SYMBOL_TYPE_V1_STOCK => SymbolType.Stock,
                SymbolConstant.SSI_SYMBOL_TYPE_V1_ETF => SymbolType.ETF,
                SymbolConstant.SSI_SYMBOL_TYPE_V1_BOND => SymbolType.BOND,
                SymbolConstant.SSI_SYMBOL_TYPE_V1_MUTUAL_FUND => SymbolType.MutualFund,
                _ => SymbolType.Unknown,
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ssiExchangeCode"></param>
        /// <returns></returns>
        private static string MapSsiExchangeCodeToDomainCode(string ssiExchangeCode)
        {
            return ssiExchangeCode.ToLower() switch
            {
                SymbolConstant.SSI_EXCHANGE_HSX => ExchangeConstant.EXCHANGE_HSX,
                SymbolConstant.SSI_EXCHANGE_HNX => ExchangeConstant.EXCHANGE_HNX,
                SymbolConstant.SSI_EXCHANGE_UPCOM => ExchangeConstant.EXCHANGE_UPCOM,
                _ => "UNKNOWN",
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ssiExchange"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<List<Symbol>> MapSsiSymbolsToDomainSymbols(string ssiExchange, CancellationToken cancellationToken)
        {
            try
            {
                var domainExchange = MapSsiExchangeCodeToDomainCode(ssiExchange);

                IEnumerable<Symbol> existedSymbols = await _uow.Symbols.FindAsync(
                    s => string.Equals(s.ExchangeCode.ToUpper(), domainExchange.ToUpper()),
                    cancellationToken);

                var existedTickers = new HashSet<string>(existedSymbols.Select(e => e.Ticker), StringComparer.OrdinalIgnoreCase);

                List<SsiSymbolDto> ssiSymbols = await _ssiService.FetchSymbolsListAsync(ssiExchange, cancellationToken);

                return [.. ssiSymbols
                    .Where(s => !string.IsNullOrEmpty(s.StockSymbol))
                    .DistinctBy(s => s.StockSymbol!.ToUpper())
                    .Where(s => !existedTickers.Contains(s.StockSymbol!.ToUpper()))
                    .Select(s => new Symbol {
                        Ticker = s.StockSymbol!,
                        Isin = s.Isin,
                        EnCompanyName = s.CompanyNameEn,
                        ViCompanyName = s.CompanyNameVi,
                        ExchangeCode = domainExchange,
                        Status = CommonStatus.Active,
                        Type = MapSsiSymbolTypeToDomainType(s.StockType)
                    })
                ];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping SSI symbols for exchange {SsiExchange}", ssiExchange);
                return [];
            }
        }
    }
}
