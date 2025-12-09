using GreenDragonTrading.Application.DTOs;
using MediatR;
using System.Text.Json.Serialization;

namespace GreenDragonTrading.Application.UseCases.Symbols.Queries.GetSymbol
{
    public record GetSymbolQuery(string Ticker) : IRequest<GetSymbolQueryResult>
    {
    }

    public record GetSymbolQueryResult(
        SymbolDto? Symbol,
        [property: JsonIgnore] string Message);
}
