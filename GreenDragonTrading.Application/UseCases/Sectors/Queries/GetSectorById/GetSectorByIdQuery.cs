using GreenDragonTrading.Application.Common.Models;
using MediatR;
using static GreenDragonTrading.Application.DTOs.SectorDtos;

namespace GreenDragonTrading.Application.UseCases.Sectors.Queries.GetSectorById
{
    /// <summary>
    /// Query to retrieve a single sector by its ID with associated symbols.
    /// </summary>
    public record GetSectorByIdQuery(string SectorId) : IRequest<ApiResponse<SectorDto>>;
}
