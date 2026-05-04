using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Domain.Enums;
using MediatR;
using static GreenDragonTrading.Application.DTOs.SectorDtos;

namespace GreenDragonTrading.Application.UseCases.Sectors.Queries.GetSectors
{
    /// <summary>
    /// Represents a query for retrieving a paginated list of sectors, optionally filtered by sector level and status.
    /// </summary>
    /// <param name="Level">The sector level to filter results by. If null, sectors of all levels are included.</param>
    /// <param name="Status">The sector status to filter results by. If null, sectors of all statuses are included.</param>
    public record GetSectorsQuery(
        int? Level,
        CommonStatus? Status = null) : PaginationQuery, IRequest<ApiResponse<PaginatedResponse<SectorDto>>>;
}
