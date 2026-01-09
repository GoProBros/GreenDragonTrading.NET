using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;
using System.Text.Json.Serialization;

namespace GreenDragonTrading.Application.UseCases.Symbols.Queries.GetSymbol
{
    /// <summary>
    /// Get a symbol details by its ticker.
    /// </summary>
    /// <param name="Ticker">Ticker symbol</param>
    public record GetSymbolQuery(string Ticker) : IRequest<ApiResponse<SymbolDto>>;
}
