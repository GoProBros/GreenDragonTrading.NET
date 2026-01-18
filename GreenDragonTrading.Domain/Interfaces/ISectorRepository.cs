using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Domain.Interfaces
{
    public interface ISectorRepository : IPostgreSqlGenericRepository<Sector>
    {
        /// <summary>
        /// Retrieves a paginated list of sectors with their associated symbols, optionally filtered by level and status.
        /// </summary>
        Task<(List<Sector> Sectors, int TotalCount)> GetSectorsWithSymbolsAsync(
            int? level,
            CommonStatus? status,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all child sector IDs at level 4 for a given parent sector.
        /// </summary>
        Task<List<string>> GetAllChildLevel4SectorIdsAsync(
            string sectorId,
            CancellationToken cancellationToken = default);
    }
}
