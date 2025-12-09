using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Symbols.Queries.GetSymbol
{
    public class GetSymbolQueryHandler(
        ILogger<GetSymbolQueryHandler> logger,
        IUnitOfWork uow) : IRequestHandler<GetSymbolQuery, GetSymbolQueryResult>
    {
        private readonly ILogger<GetSymbolQueryHandler> _logger = logger;
        private readonly IUnitOfWork _uow = uow;
        public async Task<GetSymbolQueryResult> Handle(GetSymbolQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var symbol = await _uow.Symbols.GetByIdAsync(request.Ticker, cancellationToken);

                if (symbol == null)
                {
                    _logger.LogWarning("Symbol not found for Ticker: {Ticker}", request.Ticker);
                    return new GetSymbolQueryResult (null, "Không tìm thấy mã");
                }

                var symbolDto = new SymbolDto
                {
                    Ticker = symbol.Ticker,
                    Isin = symbol.Isin,
                    EnCompanyName = symbol.EnCompanyName,
                    ViCompanyName = symbol.ViCompanyName,
                    ExchangeCode = symbol.ExchangeCode,
                    SectorId = symbol.SectorId,
                    Type = symbol.Type,
                    Status = symbol.Status
                };

                return new GetSymbolQueryResult(symbolDto, "Thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling GetSymbolQuery for Ticker: {Ticker}", request.Ticker);
                throw;
            }
        }
    }
}
