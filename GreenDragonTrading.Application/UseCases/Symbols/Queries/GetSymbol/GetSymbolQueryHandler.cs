using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Symbols.Queries.GetSymbol
{
    /// <summary>
    /// GetSymbolQueryHandler handles the retrieval of symbol information based on the provided ticker.
    /// </summary>
    /// <param name="logger">Logger</param>
    /// <param name="uow">Unit of work</param>
    public class GetSymbolQueryHandler(
        ILogger<GetSymbolQueryHandler> logger,
        IUnitOfWork uow) : IRequestHandler<GetSymbolQuery, ApiResponse<SymbolDto>>
    {
        private readonly ILogger<GetSymbolQueryHandler> _logger = logger;
        private readonly IUnitOfWork _uow = uow;

        /// <summary>
        /// Get detail of symbol by ticker handler
        /// </summary>
        /// <param name="request">Request model</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>Details data of symbol</returns>
        public async Task<ApiResponse<SymbolDto>> Handle(GetSymbolQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var symbol = await _uow.Symbols.GetByIdAsync(request.Ticker, cancellationToken);

                if (symbol == null)
                {
                    _logger.LogWarning("Symbol not found for Ticker: {Ticker}", request.Ticker);
                    return ApiResponse<SymbolDto>.Failure("Không tìm thấy mã");
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

                return ApiResponse<SymbolDto>.Success(symbolDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling GetSymbolQuery for Ticker: {Ticker}", request.Ticker);
                return ApiResponse<SymbolDto>.Failure("Lỗi xảy ra khi xử lý yêu cầu", ex.Message);
            }
        }
    }
}
