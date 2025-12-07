using GreenDragonTrading.Application.DTOs;
using MediatR;
using System.Text.Json.Serialization;

namespace GreenDragonTrading.Application.UseCases.Symbols.Queries.GetSymbols
{
    public record GetSymbolsQuery(
        int PageIndex,
        int PageSize,
        int? Type,
        string? Exchange,
        string? Sector) : IRequest<GetSymbolsQueryResult>;

    public record GetSymbolsQueryResult(
        List<SymbolDtos> Symbols,
        int TotalCount,
        [property: JsonIgnore] string Message
    );
}
