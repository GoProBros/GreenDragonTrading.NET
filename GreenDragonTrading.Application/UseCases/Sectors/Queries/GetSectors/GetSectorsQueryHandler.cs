using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using static GreenDragonTrading.Application.DTOs.SectorDtos;

namespace GreenDragonTrading.Application.UseCases.Sectors.Queries.GetSectors
{
    /// <summary>
    /// Handles queries for retrieving sectors.
    /// </summary>
    public class GetSectorsQueryHandler(
        ILogger<GetSectorsQueryHandler> logger,
        IUnitOfWork unitOfWork) : IRequestHandler<GetSectorsQuery, ApiResponse<PaginatedResponse<SectorDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<GetSectorsQueryHandler> _logger = logger;

        public async Task<ApiResponse<PaginatedResponse<SectorDto>>> Handle(GetSectorsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting sectors with Level={Level}, Status={Status}, PageIndex={PageIndex}, PageSize={PageSize}",
                request.Level, request.Status, request.PageIndex, request.PageSize);

            // Retrieve paginated sectors from repository
            var (sectors, totalCount) = await _unitOfWork.Sectors.GetSectorsWithSymbolsAsync(
                request.Level,
                request.Status,
                request.PageIndex,
                request.PageSize,
                cancellationToken);

            // Map to DTOs
            var sectorDtos = new List<SectorDto>();

            foreach (var sector in sectors)
            {
                var symbolTickers = new List<string>();

                // If sector is level 4, get symbols directly
                if (sector.Level == 4)
                {
                    symbolTickers = sector.Symbols.Select(s => s.Ticker).ToList();
                }
                // If sector is not level 4, find all level 4 child sectors and get their symbols
                else if (sector.Level < 4)
                {
                    // Get all level 4 child sector IDs
                    var childLevel4SectorIds = await _unitOfWork.Sectors.GetAllChildLevel4SectorIdsAsync(
                        sector.Id,
                        cancellationToken);

                    // Get all symbols belonging to level 4 child sectors
                    if (childLevel4SectorIds.Count > 0)
                    {
                        var symbols = await _unitOfWork.Symbols.FindAsync(
                            s => s.SectorId != null && childLevel4SectorIds.Contains(s.SectorId),
                            cancellationToken);

                        symbolTickers = symbols.Select(s => s.Ticker).Distinct().ToList();
                    }
                }

                sectorDtos.Add(new SectorDto
                {
                    Id = sector.Id,
                    EnName = sector.EnName,
                    ViName = sector.ViName,
                    Level = sector.Level,
                    Status = sector.Status,
                    Symbols = symbolTickers
                });
            }

            _logger.LogInformation("Retrieved {Count} sectors out of {TotalCount}",
                sectorDtos.Count, totalCount);

            var paginatedResponse = PaginatedResponse<SectorDto>.Create(
                sectorDtos,
                totalCount,
                request.PageIndex,
                request.PageSize);

            return ApiResponse<PaginatedResponse<SectorDto>>.Success(
                paginatedResponse,
                "Sectors retrieved successfully");
        }
    }
}
