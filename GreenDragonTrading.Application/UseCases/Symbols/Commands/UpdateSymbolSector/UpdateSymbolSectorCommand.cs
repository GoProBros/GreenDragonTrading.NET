using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Symbols.Commands.UpdateSymbolSector
{
    /// <summary>
    /// Command to update a symbol's sector. Only level 4 sectors are allowed.
    /// </summary>
    public record UpdateSymbolSectorCommand : IRequest<ApiResponse<SymbolDto>>
    {
        public string Ticker { get; init; } = null!;
        public string? SectorId { get; init; }
    }
}
