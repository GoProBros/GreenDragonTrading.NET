using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Symbols.Commands.UpdateSymbolSector
{
    /// <summary>
    /// Handler for updating a symbol's sector.
    /// </summary>
    public class UpdateSymbolSectorCommandHandler(
        ILogger<UpdateSymbolSectorCommandHandler> logger,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdateSymbolSectorCommand, ApiResponse<SymbolDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<UpdateSymbolSectorCommandHandler> _logger = logger;

        public async Task<ApiResponse<SymbolDto>> Handle(UpdateSymbolSectorCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating sector for symbol {Ticker} to {SectorId}", request.Ticker, request.SectorId);

            // Get the symbol
            var symbol = await _unitOfWork.Symbols.GetByIdAsync(request.Ticker, cancellationToken);
            if (symbol == null || symbol.Status == CommonStatus.InActive)
            {
                throw new NotFoundException($"Symbol with ticker {request.Ticker} not found");
            }

            // Validate sector if provided
            if (!string.IsNullOrEmpty(request.SectorId))
            {
                var sector = await _unitOfWork.Sectors.GetByIdAsync(request.SectorId, cancellationToken);
                if (sector == null || sector.Status == CommonStatus.InActive)
                {
                    throw new NotFoundException($"Sector with ID {request.SectorId} not found");
                }

                // Only allow level 4 sectors
                if (sector.Level != 4)
                {
                    throw new BusinessRuleException("Only level 4 sectors can be assigned to symbols");
                }
            }

            // Update the symbol's sector
            symbol.SectorId = request.SectorId;
            _unitOfWork.Symbols.Update(symbol);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Symbol sector updated successfully: {Ticker}", symbol.Ticker);

            var symbolDto = new SymbolDto
            {
                Ticker = symbol.Ticker,
                Isin = symbol.Isin,
                EnCompanyName = symbol.EnCompanyName,
                ViCompanyName = symbol.ViCompanyName,
                ExchangeCode = symbol.ExchangeCode,
                SectorId = symbol.SectorId,
                Type = symbol.Type,
                Status = symbol.Status
            };

            return ApiResponse<SymbolDto>.Success(symbolDto, "Symbol sector updated successfully");
        }
    }
}
