using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Sectors.Commands.DeleteSector
{
    /// <summary>
    /// Handler for soft deleting a sector.
    /// </summary>
    public class DeleteSectorCommandHandler(
        ILogger<DeleteSectorCommandHandler> logger,
        IUnitOfWork unitOfWork) : IRequestHandler<DeleteSectorCommand, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<DeleteSectorCommandHandler> _logger = logger;

        public async Task<ApiResponse> Handle(DeleteSectorCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting sector with ID={SectorId}", request.SectorId);

            var sector = await _unitOfWork.Sectors.GetByIdAsync(request.SectorId, cancellationToken);

            if (sector == null || sector.Status == CommonStatus.InActive)
            {
                throw new NotFoundException($"Sector with ID {request.SectorId} not found");
            }

            // Check if sector has active child sectors
            var activeChildren = await _unitOfWork.Sectors.FindAsync(
                s => s.ParentId == sector.Id && s.Status == CommonStatus.Active,
                cancellationToken);

            if (activeChildren.Any())
            {
                throw new BusinessRuleException("Cannot delete sector with active child sectors");
            }

            // Soft delete by setting status to InActive
            sector.Status = CommonStatus.InActive;
            _unitOfWork.Sectors.Update(sector);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Sector soft deleted successfully: {SectorId}", sector.Id);

            return ApiResponse.Success("Sector deleted successfully");
        }
    }
}
