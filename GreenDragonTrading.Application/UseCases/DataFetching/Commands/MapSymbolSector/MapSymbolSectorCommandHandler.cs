using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.MapSymbolSector
{
    public class MapSymbolSectorCommandHandler(
        IUnitOfWork uow,
        ISsiServiceV1 ssiV1,
        ILogger<MapSymbolSectorCommandHandler> logger) : IRequestHandler<MapSymbolSectorCommand, MapSymbolSectorCommandResult>
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly ISsiServiceV1 _ssiV1 = ssiV1;
        private readonly ILogger<MapSymbolSectorCommandHandler> _logger = logger;

        public async Task<MapSymbolSectorCommandResult> Handle(MapSymbolSectorCommand request, CancellationToken cancellationToken)
        {
            try 
            {
                int totalCount = 0;
                var sectors = await _ssiV1.FetchIndustryListAsync(4, cancellationToken);
                if (sectors == null || sectors.Count <= 0)
                {
                    _logger.LogWarning("No sectors found from SSI");
                    return new MapSymbolSectorCommandResult(0, "No sectors found from SSI");
                }
                foreach (var sector in sectors)
                {
                    if (sector == null || sector.CompanyList == null || sector.CompanyList.Count <= 0)
                    {
                        _logger.LogWarning("No companies found in sector {SectorName}", sector?.IndustryName);
                        continue;
                    }
                    var isSectorExisted = await _uow.Sectors.FindAsync(s => s.Id == sector.IndustryCode, cancellationToken);
                    if (isSectorExisted == null)
                    {
                        _logger.LogWarning("Sector with empty name found {Code}", sector.IndustryCode);
                        continue;
                    }
                    var symbols = sector.CompanyList;
                    if(symbols == null || symbols.Count <= 0)
                    {
                        _logger.LogWarning("No symbols found in sector {SectorName}", sector.IndustryName);
                        continue;
                    }
                    foreach (var symbol in symbols)
                    {
                        var existedSymbol = await _uow.Symbols.GetByIdAsync(symbol.Symbol!, cancellationToken);
                        if (existedSymbol == null || existedSymbol!.SectorId == sector.IndustryCode)
                        {
                            continue;
                        }
                        totalCount++;
                        existedSymbol.SectorId = sector.IndustryCode;

                        _uow.Symbols.Update(existedSymbol);
                    }
                    await _uow.SaveChangesAsync(cancellationToken);
                }
                _logger.LogInformation("Successfully mapped {TotalCount} symbols sector from SSI", totalCount);
                return new MapSymbolSectorCommandResult(totalCount, "Successfully mapped symbols sector from SSI");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing symbols from SSI");
                return new MapSymbolSectorCommandResult(0, "Error mapping symbols sector from SSI");
            }
        }
    }
}
