using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    public class SectorRepository(GdtPostgreSqlDbContext context) : PostgreSqlGenericRepository<Sector>(context), ISectorRepository
    {
        /// <summary>
        /// Retrieves a paginated list of sectors with their associated symbols, optionally filtered by level and status.
        /// </summary>
        public async Task<(List<Sector> Sectors, int TotalCount)> GetSectorsWithSymbolsAsync(
            int? level,
            Domain.Enums.CommonStatus? status,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Sectors
                .Include(s => s.Symbols)
                .AsQueryable();

            // Filter by status if provided, otherwise show only active
            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }
            else
            {
                query = query.Where(s => s.Status == Domain.Enums.CommonStatus.Active);
            }

            // Filter by level if provided
            if (level.HasValue)
            {
                query = query.Where(s => s.Level == level.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var sectors = await query
                .OrderBy(s => s.Level)
                .ThenBy(s => s.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (sectors, totalCount);
        }

        /// <summary>
        /// Gets all child sector IDs at level 4 for a given parent sector.
        /// Uses LINQ-based recursive approach to traverse the sector hierarchy.
        /// </summary>
        public async Task<List<string>> GetAllChildLevel4SectorIdsAsync(
            string sectorId,
            CancellationToken cancellationToken = default)
        {
            // Get the current sector
            var currentSector = await _context.Sectors
                .FirstOrDefaultAsync(s => s.Id == sectorId, cancellationToken);

            if (currentSector == null)
            {
                return [];
            }

            // If current sector is already level 4, return itself
            if (currentSector.Level == 4)
            {
                return [sectorId];
            }

            // Load all sectors into memory to traverse hierarchy
            var allSectors = await _context.Sectors
                .AsNoTracking()
                .Where(s => s.Status == Domain.Enums.CommonStatus.Active) // Only active sectors
                .ToListAsync(cancellationToken);

            // Find all level 4 child sectors using recursive helper
            var level4SectorIds = GetAllLevel4Children(sectorId, allSectors);

            return level4SectorIds;
        }

        /// <summary>
        /// Recursive helper method to find all level 4 child sectors.
        /// </summary>
        private List<string> GetAllLevel4Children(string parentId, List<Sector> allSectors)
        {
            var result = new List<string>();

            // Find all direct children
            var children = allSectors.Where(s => s.ParentId == parentId).ToList();

            foreach (var child in children)
            {
                // If it's level 4, add to result
                if (child.Level == 4)
                {
                    result.Add(child.Id);
                }
                // If not level 4 yet, continue finding its children
                else if (child.Level < 4)
                {
                    result.AddRange(GetAllLevel4Children(child.Id, allSectors));
                }
            }

            return result;
        }
    }
}
