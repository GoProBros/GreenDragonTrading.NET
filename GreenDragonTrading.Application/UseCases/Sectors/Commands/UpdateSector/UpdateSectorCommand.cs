using GreenDragonTrading.Application.Common.Models;
using MediatR;
using static GreenDragonTrading.Application.DTOs.SectorDtos;

namespace GreenDragonTrading.Application.UseCases.Sectors.Commands.UpdateSector
{
    /// <summary>
    /// Command to update an existing sector.
    /// </summary>
    public record UpdateSectorCommand : IRequest<ApiResponse<SectorDto>>
    {
        public string Id { get; init; } = null!;
        public string? EnName { get; init; }
        public string? ViName { get; init; }
    }
}
