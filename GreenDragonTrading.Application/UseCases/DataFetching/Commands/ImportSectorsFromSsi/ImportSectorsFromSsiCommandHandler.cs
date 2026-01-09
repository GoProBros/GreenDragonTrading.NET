using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportSectorsFromSsi
{
    public class ImportSectorsFromSsiCommandHandler(
        ISsiServiceV1 ssiService,
        IUnitOfWork uow,
        ILogger<ImportSectorsFromSsiCommandHandler> logger) : IRequestHandler<ImportSectorsFromSsiCommand, ImportSectorsFromSsiResult>
    {
        private readonly ISsiServiceV1 _ssiService = ssiService;
        private readonly IUnitOfWork _uow = uow;
        private readonly ILogger<ImportSectorsFromSsiCommandHandler> _logger = logger;

        /// <summary>
        /// Handles the import of sector data from SSI API and saves to database.
        /// </summary>
        /// <param name="request">Import command for sectors.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Result of imported and updated sectors.</returns>
        public async Task<ImportSectorsFromSsiResult> Handle(ImportSectorsFromSsiCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var industryList = await FetchIndustryListAsync(cancellationToken);
                if (industryList.Count == 0)
                    return new ImportSectorsFromSsiResult(0, 0, "No industries found from SSI to import.");

                int totalAdded = 0;
                int totalUpdated = 0;

                for (int i = 1; i <= 4; i++)
                {
                    var (added, updated) = await ProcessSectorsForLevel(industryList, i, cancellationToken);
                    totalAdded += added;
                    totalUpdated += updated;
                }

                if (totalAdded == 0 && totalUpdated == 0)
                    return new ImportSectorsFromSsiResult(0, 0, "No new or updated sectors from SSI.");

                await _uow.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Imported {AddedCount} and updated {UpdatedCount} sectors from SSI.", totalAdded, totalUpdated);
                return new ImportSectorsFromSsiResult(totalAdded, totalUpdated, $"Successfully imported {totalAdded} and updated {totalUpdated} sectors from SSI.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing sectors from SSI: {Message}", ex.Message);
                return new ImportSectorsFromSsiResult(0, 0, $"Error importing sectors: {ex.Message}");
            }
        }

        /// <summary>
        /// Fetches the list of industries from SSI API through the SSI service.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A list of <see cref="SsiIndustryDto"/> containing industry data from SSI.</returns>
        private async Task<List<SsiIndustryDto>> FetchIndustryListAsync(CancellationToken cancellationToken)
        {
            return await _ssiService.FetchIndustryListAsync(null, cancellationToken);
        }

        private async Task<(int added, int updated)> ProcessSectorsForLevel(List<SsiIndustryDto> industryList, int level, CancellationToken cancellationToken)
        {
            var existingSectors = await _uow.Sectors.FindAsync(s => s.Level == level, cancellationToken);
            var existingSectorsDict = existingSectors.ToDictionary(s => s.Id, s => s);

            var ssiSectorsForLevel = industryList
                .Where(s => int.Parse(s.IndustryLevel!) == level)
                .ToList();

            List<Sector> sectorsToAdd = [];
            List<Sector> sectorsToUpdate = [];

            foreach (var industryDto in ssiSectorsForLevel)
            {
                var sectorId = industryDto.IndustryCode!;
                if (existingSectorsDict.TryGetValue(sectorId, out var existingSector))
                {
                    if (IsSectorChanged(existingSector, industryDto))
                    {
                        UpdateSector(existingSector, industryDto);
                        sectorsToUpdate.Add(existingSector);
                    }
                }
                else
                {
                    sectorsToAdd.Add(CreateSector(industryDto, level));
                }
            }

            if (sectorsToAdd.Count > 0)
            {
                await _uow.Sectors.AddRangeAsync(sectorsToAdd, cancellationToken);
                _logger.LogInformation("Added {Count} new sectors at level {Level}.", sectorsToAdd.Count, level);
            }

            if (sectorsToUpdate.Count > 0)
            {
                _uow.Sectors.UpdateRange(sectorsToUpdate);
                _logger.LogInformation("Updated {Count} sectors at level {Level}.", sectorsToUpdate.Count, level);
            }

            return (sectorsToAdd.Count, sectorsToUpdate.Count);
        }

        /// <summary>
        /// Determines if a sector entity has changed compared to the SSI industry data.
        /// Checks for differences in English name, Vietnamese name, and parent sector ID.
        /// </summary>
        /// <param name="existingSector">The existing sector entity from the database.</param>
        /// <param name="industryDto">The industry DTO from SSI API.</param>
        /// <returns>True if any property has changed; otherwise, false.</returns>
        private static bool IsSectorChanged(Sector existingSector, SsiIndustryDto industryDto)
        {
            return existingSector.EnName != industryDto.IndustryName?.En
                || existingSector.ViName != industryDto.IndustryName?.Vi
                || existingSector.ParentId != GetParentId(industryDto.IndustryCode!, industryDto.IndustryLevel!);
        }

        /// <summary>
        /// Updates an existing sector entity with data from SSI industry DTO.
        /// Updates English name, Vietnamese name, and parent sector ID.
        /// </summary>
        /// <param name="sector">The sector entity to update.</param>
        /// <param name="industryDto">The industry DTO containing new data from SSI.</param>
        private static void UpdateSector(Sector sector, SsiIndustryDto industryDto)
        {
            sector.EnName = industryDto.IndustryName?.En;
            sector.ViName = industryDto.IndustryName?.Vi;
            sector.ParentId = GetParentId(industryDto.IndustryCode!, industryDto.IndustryLevel!);
        }

        /// <summary>
        /// Creates a new sector entity from SSI industry DTO.
        /// Maps industry code, names, level, and calculates parent sector ID.
        /// </summary>
        /// <param name="industryDto">The industry DTO from SSI API.</param>
        /// <param name="level">The hierarchy level of the sector (1-4).</param>
        /// <returns>A new <see cref="Sector"/> entity populated with SSI data.</returns>
        private static Sector CreateSector(SsiIndustryDto industryDto, int level)
        {
            return new Sector
            {
                Id = industryDto.IndustryCode!,
                EnName = industryDto.IndustryName?.En,
                ViName = industryDto.IndustryName?.Vi,
                Level = level,
                ParentId = GetParentId(industryDto.IndustryCode!, industryDto.IndustryLevel!)
            };
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
