using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportSectorsFromSsi
{
    public class ImportSectorFromSsiCommandHandler(
        ISsiService ssiService,
        IUnitOfWork uow,
        ILogger<ImportSectorFromSsiCommandHandler> logger) : IRequestHandler<ImportSectorFromSsiCommand, ImportSectorsFromSsiResult>
    {
        private readonly ISsiService _ssiService = ssiService;
        private readonly IUnitOfWork _uow = uow;
        private readonly ILogger<ImportSectorFromSsiCommandHandler> _logger = logger;

        /// <summary>
        /// Handle import sectors data from SSI api
        /// </summary>
        /// <param name="request">Request record</param>
        /// <param name="cancellationToken">Cancellation tolen</param>
        /// <returns>Sectors data base on SSI inserted to database</returns>
        public async Task<ImportSectorsFromSsiResult> Handle(ImportSectorFromSsiCommand request, CancellationToken cancellationToken)
        {
            try
            {
                List<SsiIndustryDto> industryList = await _ssiService.FetchIndustryListAsync(cancellationToken);

                if (industryList.Count == 0)
                {
                    return new ImportSectorsFromSsiResult(0, "No industries found from SSI to import.");
                }

                int totalSectors = 0;

                for (int i = 1; i <= 4; i++)
                {
                    var existingSectors = await _uow.Sectors.FindAsync(s => s.Level == i, cancellationToken);

                    List<Sector> sectors = [.. industryList
                    .Where(s => int.Parse(s.IndustryLevel!) == i)
                    .Where(s => !existingSectors.Any(es => string.Equals(es.Id, s.IndustryCode)))
                    .Select(industryDto => new Sector
                    {
                        Id = industryDto.IndustryCode!,
                        EnName = industryDto.IndustryName?.En,
                        ViName = industryDto.IndustryName?.Vi,
                        Level = industryDto.IndustryLevel != null ? int.Parse(industryDto.IndustryLevel) : null,
                        ParentId = GetParentId(industryDto.IndustryCode!, industryDto.IndustryLevel!)
                    })];

                    await _uow.Sectors.AddRangeAsync(sectors, cancellationToken);
                    totalSectors += sectors.Count;
                    _logger.LogInformation("Imported {Count} sectors level {Level}.", sectors.Count, i);
                }

                if(totalSectors == 0)
                {
                    return new ImportSectorsFromSsiResult(0, "No new sectors to import from SSI.");
                }

                await _uow.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Imported {Count} sectors from SSI.", totalSectors);
                return new ImportSectorsFromSsiResult(totalSectors, "Imported sectors successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing sectors from SSI: {Message}", ex.Message);
                return new ImportSectorsFromSsiResult(0, $"Error importing sectors: {ex.Message}");
            }
        }

        /// <summary>
        /// Get parent id for current sector if level greater than 2
        /// </summary>
        /// <param name="id">Id of current sector</param>
        /// <param name="level">Level of current sector</param>
        /// <returns>Parent sector id of current sector</returns>
        private static string? GetParentId(string id, string level)
        {
            var lvl = int.Parse(level);

            if (lvl <= SectorConstants.MIN_LEVEL_SECTOR) return null;

            int digitsToZero = (SectorConstants.MAX_LEVEL_SECTOR + 1) - lvl;

            char[] arr = id.ToCharArray();

            for (int i = 0; i < digitsToZero; i++)
            {
                int pos = arr.Length - 1 - i;
                if (pos >= 0) arr[pos] = '0';
            }

            if (arr.All(c => c == '0'))
            {
                return SectorConstants.SECTOR_OIL_GAS;
            }

            if (string.Equals(id, "8980")) return SectorConstants.SECTOR_FINANCIAL_SERVICES;

            return new string(arr);
        }
    }
}
