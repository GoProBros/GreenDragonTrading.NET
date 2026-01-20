using GreenDragonTrading.Application.Common.Models;
using MediatR;
using static GreenDragonTrading.Application.DTOs.SectorDtos;

namespace GreenDragonTrading.Application.UseCases.Sectors.Commands.CreateSector
{
    /// <summary>
    /// Command to create a new sector.
    /// </summary>
    public record CreateSectorCommand : IRequest<ApiResponse<SectorDto>>
    {
        public string Id { get; init; } = null!;
        public string? EnName { get; init; }
        public string? ViName { get; init; }
        public string? ParentId { get; init; }
        public int Level { get; init; }
    }
}
