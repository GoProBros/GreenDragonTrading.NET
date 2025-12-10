using GreenDragonTrading.Application.DTOs;
using MediatR;
using System.Text.Json.Serialization;

namespace GreenDragonTrading.Application.UseCases.Symbols.Queries.GetSymbols
{
    /// <summary>
    /// Get a paginated list of symbols with optional filtering by type, exchange, and sector.
    /// </summary>
    /// <param name="PageIndex">Page index</param>
    /// <param name="PageSize">Number of items per page</param>
    /// <param name="Type">Filter by symbol type</param>
    /// <param name="Exchange">Filter by exchange</param>
    /// <param name="Sector">Filter by sector</param>
    public record GetSymbolsQuery(
        int PageIndex,
        int PageSize,
        int? Type,
        string? Exchange,
        string? Sector) : IRequest<GetSymbolsQueryResult>;

    /// <summary>
    /// Paginated result of symbols.
    /// </summary>
    /// <param name="Symbols">List of symbols</param>
    /// <param name="TotalCount">Total record count</param>
    /// <param name="Message">Optional message</param>
    public record GetSymbolsQueryResult(
        List<SymbolDto> Symbols,
        int TotalCount,
        [property: JsonIgnore] string Message
    );
}
