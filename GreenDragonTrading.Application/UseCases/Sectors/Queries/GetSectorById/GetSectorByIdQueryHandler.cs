using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using static GreenDragonTrading.Application.DTOs.SectorDtos;

namespace GreenDragonTrading.Application.UseCases.Sectors.Queries.GetSectorById
{
    /// <summary>
    /// Handler for retrieving a single sector by ID.
    /// </summary>
    public class GetSectorByIdQueryHandler(
        ILogger<GetSectorByIdQueryHandler> logger,
        IUnitOfWork unitOfWork) : IRequestHandler<GetSectorByIdQuery, ApiResponse<SectorDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<GetSectorByIdQueryHandler> _logger = logger;

        public async Task<ApiResponse<SectorDto>> Handle(GetSectorByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting sector with ID={SectorId}", request.SectorId);

            var sector = await _unitOfWork.Sectors.GetByIdAsync(request.SectorId, cancellationToken);

            if (sector == null || sector.Status == CommonStatus.InActive)
            {
                throw new NotFoundException($"Sector with ID {request.SectorId} not found");
            }

            // Get symbols for this sector
            var symbolTickers = new List<string>();

            if (sector.Level == 4)
            {
                // Level 4 sector: get symbols directly
                var symbols = await _unitOfWork.Symbols.FindAsync(
                    s => s.SectorId == sector.Id && s.Status == CommonStatus.Active,
                    cancellationToken);
                symbolTickers = symbols.Select(s => s.Ticker).ToList();
            }
            else if (sector.Level < 4)
            {
                // Lower level sector: get all level 4 children and their symbols
                var childLevel4SectorIds = await _unitOfWork.Sectors.GetAllChildLevel4SectorIdsAsync(
                    sector.Id,
                    cancellationToken);

                if (childLevel4SectorIds.Count > 0)
                {
                    var symbols = await _unitOfWork.Symbols.FindAsync(
                        s => s.SectorId != null && childLevel4SectorIds.Contains(s.SectorId) && s.Status == CommonStatus.Active,
                        cancellationToken);

                    symbolTickers = symbols.Select(s => s.Ticker).Distinct().ToList();
                }
            }

            var sectorDto = new SectorDto
            {
                Id = sector.Id,
                EnName = sector.EnName,
                ViName = sector.ViName,
                Level = sector.Level,
                Status = sector.Status,
                Symbols = symbolTickers
            };

            return ApiResponse<SectorDto>.Success(sectorDto, "Sector retrieved successfully");
        }
    }
}
