using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Sectors.Commands.DeleteSector
{
    /// <summary>
    /// Command to soft delete a sector (set status to Inactive).
    /// </summary>
    public record DeleteSectorCommand(string SectorId) : IRequest<ApiResponse>;
}
