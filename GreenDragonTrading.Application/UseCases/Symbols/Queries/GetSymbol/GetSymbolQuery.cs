using GreenDragonTrading.Application.DTOs;
using MediatR;
using System.Text.Json.Serialization;

namespace GreenDragonTrading.Application.UseCases.Symbols.Queries.GetSymbol
{
    /// <summary>
    /// Get a symbol details by its ticker.
    /// </summary>
    /// <param name="Ticker">Ticker symbol</param>
    public record GetSymbolQuery(string Ticker) : IRequest<GetSymbolQueryResult>
    {
    }

    /// <summary>
    /// Represents the result of a symbol query, including the symbol data and an associated message.
    /// </summary>
    /// <param name="Symbol">The symbol data returned by the query, or <see langword="null"/> if no symbol was found.</param>
    /// <param name="Message">A message providing additional information about the query result, such as error details or status.</param>
    public record GetSymbolQueryResult(
        SymbolDto? Symbol,
        [property: JsonIgnore] string Message);
}
