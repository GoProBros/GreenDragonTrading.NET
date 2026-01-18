using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using static GreenDragonTrading.Application.DTOs.SectorDtos;

namespace GreenDragonTrading.Application.UseCases.Sectors.Commands.CreateSector
{
    /// <summary>
    /// Handler for creating a new sector.
    /// </summary>
    public class CreateSectorCommandHandler(
        ILogger<CreateSectorCommandHandler> logger,
        IUnitOfWork unitOfWork) : IRequestHandler<CreateSectorCommand, ApiResponse<SectorDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<CreateSectorCommandHandler> _logger = logger;

        public async Task<ApiResponse<SectorDto>> Handle(CreateSectorCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating sector with ID={SectorId}", request.Id);

            // Check if sector already exists
            var existingSector = await _unitOfWork.Sectors.GetByIdAsync(request.Id, cancellationToken);
            if (existingSector != null)
            {
                throw new ConflictException($"Sector with ID {request.Id} already exists");
            }

            // Validate parent sector if provided
            if (!string.IsNullOrEmpty(request.ParentId))
            {
                var parentSector = await _unitOfWork.Sectors.GetByIdAsync(request.ParentId, cancellationToken);
                if (parentSector == null || parentSector.Status == CommonStatus.InActive)
                {
                    throw new NotFoundException($"Parent sector with ID {request.ParentId} not found");
                }

                // Validate parent level
                if (parentSector.Level != request.Level - 1)
                {
                    throw new BusinessRuleException($"Parent sector must be level {request.Level - 1}");
                }
            }

            var sector = new Sector
            {
                Id = request.Id,
                EnName = request.EnName,
                ViName = request.ViName,
                ParentId = request.ParentId,
                Level = request.Level,
                Status = CommonStatus.Active
            };

            await _unitOfWork.Sectors.AddAsync(sector, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Sector created successfully: {SectorId}", sector.Id);

            var sectorDto = new SectorDto
            {
                Id = sector.Id,
                EnName = sector.EnName,
                ViName = sector.ViName,
                Level = sector.Level,
                Status = sector.Status,
                Symbols = []
            };

            return ApiResponse<SectorDto>.Success(sectorDto, "Sector created successfully");
        }
    }
}
