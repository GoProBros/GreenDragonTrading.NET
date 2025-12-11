using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Symbols.Queries.SearchSymbols
{
    /// <summary>
    /// Search symbols query handler.
    /// </summary>
    public class SearchSymbolsQueryHandler(
        IUnitOfWork uow,
        ILogger<SearchSymbolsQueryHandler> logger) : IRequestHandler<SearchSymbolsQuery, ApiResponse<PaginatedResponse<SimpleSymbolDto>>>
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly ILogger<SearchSymbolsQueryHandler> _logger = logger;

        /// <summary>
        /// Handle business logic to search symbols.
        /// </summary>
        /// <param name="request">Request model containing search parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task<ApiResponse<PaginatedResponse<SimpleSymbolDto>>> Handle(SearchSymbolsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Searching symbols with query: {Query}, IsTickerOnly: {IsTickerOnly}, PageIndex: {PageIndex}, PageSize: {PageSize}",
                    request.Query, request.IsTickerOnly, request.PageIndex, request.PageSize);

                var (symbols, totalCount) = await _uow.Symbols.SearchSymbolsAsync(
                    request.Query ?? String.Empty,
                    request.IsTickerOnly,
                    request.PageIndex,
                    request.PageSize,
                    cancellationToken);

                var simpleSymbols = symbols.Select(s => new SimpleSymbolDto
                {
                    Ticker = s.Ticker,
                    ViCompanyName = s.ViCompanyName,
                    EnCompanyName = s.EnCompanyName
                }).ToList();

                return ApiResponse<PaginatedResponse<SimpleSymbolDto>>.Success(PaginatedResponse<SimpleSymbolDto>.Create(simpleSymbols, totalCount, request.PageIndex, request.PageSize));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurred while searching symbols with query: {Query}", request.Query);
                throw;
            }
        }
    }
}
