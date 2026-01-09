using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;
using System.Text.Json.Serialization;

namespace GreenDragonTrading.Application.UseCases.Symbols.Queries.GetSymbols
{
    /// <summary>
    /// Get a paginated list of symbols with optional filtering by type, exchange, and sector.
    /// </summary>
    /// <param name="Type">Filter by symbol type. 1 = Stock, 2 = ETF, 3 = Bond, 4 = Futures. Leave empty for all types.</param>
    /// <param name="Exchange">Filter by exchange code (e.g., "HSX", "HNX", "UPCOM"). Leave empty for all exchanges.</param>
    /// <param name="Sector">Filter by sector ID. Leave empty for all sectors.</param>
    public record GetSymbolsQuery(
        int? Type,
        string? Exchange,
        string? Sector) : PaginationQuery, IRequest<ApiResponse<PaginatedResponse<SymbolDto>>>;
}
