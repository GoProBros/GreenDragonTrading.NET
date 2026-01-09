using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Symbols.Queries.SearchSymbols
{
    /// <summary>
    /// Search for symbols matching the given query.
    /// </summary>
    /// <param name="Query">Search keyword to match against ticker symbol or company name.</param>
    /// <param name="IsTickerOnly">If true, search only by ticker symbol. If false, search by both ticker and company name.</param>
    public record SearchSymbolsQuery(string? Query, bool IsTickerOnly) : PaginationQuery, IRequest<ApiResponse<PaginatedResponse<SimpleSymbolDto>>>;
}
