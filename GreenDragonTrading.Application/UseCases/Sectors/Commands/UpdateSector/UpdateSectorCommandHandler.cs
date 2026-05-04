using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using static GreenDragonTrading.Application.DTOs.SectorDtos;

namespace GreenDragonTrading.Application.UseCases.Sectors.Commands.UpdateSector
{
    /// <summary>
    /// Handler for updating an existing sector.
    /// </summary>
    public class UpdateSectorCommandHandler(
        ILogger<UpdateSectorCommandHandler> logger,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdateSectorCommand, ApiResponse<SectorDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<UpdateSectorCommandHandler> _logger = logger;

        public async Task<ApiResponse<SectorDto>> Handle(UpdateSectorCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating sector with ID={SectorId}", request.Id);

            var sector = await _unitOfWork.Sectors.GetByIdAsync(request.Id, cancellationToken);

            if (sector == null || sector.Status == CommonStatus.InActive)
            {
                throw new NotFoundException($"Sector with ID {request.Id} not found");
            }

            // Update sector properties
            sector.EnName = request.EnName;
            sector.ViName = request.ViName;

            _unitOfWork.Sectors.Update(sector);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Sector updated successfully: {SectorId}", sector.Id);

            // Get symbols for response
            var symbolTickers = new List<string>();
            if (sector.Level == 4)
            {
                var symbols = await _unitOfWork.Symbols.FindAsync(
                    s => s.SectorId == sector.Id && s.Status == CommonStatus.Active,
                    cancellationToken);
                symbolTickers = symbols.Select(s => s.Ticker).ToList();
            }
            else if (sector.Level < 4)
            {
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

            return ApiResponse<SectorDto>.Success(sectorDto, "Sector updated successfully");
        }
    }
}
