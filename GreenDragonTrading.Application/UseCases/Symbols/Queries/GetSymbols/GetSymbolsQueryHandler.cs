using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Symbols.Queries.GetSymbols
{
    /// <summary>
    /// Get symbols with pagination and optional filters handling
    /// </summary>
    /// <param name="logger">Logger</param>
    /// <param name="unitOfWork">Unit of work</param>
    public class GetSymbolsQueryHandler(
        ILogger<GetSymbolsQueryHandler> logger,
        IUnitOfWork unitOfWork) : IRequestHandler<GetSymbolsQuery, ApiResponse<PaginatedResponse<SymbolDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<GetSymbolsQueryHandler> _logger = logger;

        /// <summary>
        /// Get symbols with pagination and optional filters handling
        /// </summary>
        /// <param name="request">Request model</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>A list of symbols</returns>
        public async Task<ApiResponse<PaginatedResponse<SymbolDto>>> Handle(GetSymbolsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var symbolType = request.Type.HasValue ? (SymbolType?)request.Type.Value : null;

                var (symbols, totalCount) = await _unitOfWork.Symbols.GetSymbolsAsync(
                    request.PageIndex,
                    request.PageSize,
                    symbolType,
                    request.Exchange,
                    request.Sector,
                    cancellationToken);

                var symbolDtos = symbols.Select(s => new SymbolDto
                {
                    Ticker = s.Ticker,
                    Isin = s.Isin,
                    EnCompanyName = s.EnCompanyName,
                    ViCompanyName = s.ViCompanyName,
                    ExchangeCode = s.ExchangeCode,
                    SectorId = s.SectorId,
                    Type = s.Type,
                    Status = s.Status
                }).ToList();

                return ApiResponse<PaginatedResponse<SymbolDto>>.Success(PaginatedResponse<SymbolDto>.Create(symbolDtos, totalCount, request.PageIndex, request.PageSize));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling GetSymbolsQuery");
                throw;
            }
        }
    }
}
