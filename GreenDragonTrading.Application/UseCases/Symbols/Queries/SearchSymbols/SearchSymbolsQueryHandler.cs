using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Entities;
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
        ILogger<SearchSymbolsQueryHandler> logger) : IRequestHandler<SearchSymbolsQuery, SearchSymbolsQueryResult>
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly ILogger<SearchSymbolsQueryHandler> _logger = logger;

        /// <summary>
        /// Handle business logic to search symbols.
        /// </summary>
        /// <param name="request">Request model containing search parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task<SearchSymbolsQueryResult> Handle(SearchSymbolsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                IEnumerable<Symbol> symbols = await _uow.Symbols.SearchSymbolsAsync(request.Query, request.IsTickerOnly, cancellationToken);
                return new SearchSymbolsQueryResult
                (
                    [.. symbols.Select(s => new SimpleSymbolDto
                    {
                        Ticker = s.Ticker,
                        EnCompanyName = s.EnCompanyName,
                        ViCompanyName = s.ViCompanyName,
                    })]
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching symbols with query: {Query}", request.Query);
                throw;
            }
        }
    }
}
