using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Domain.Interfaces
{
    public interface IFinancialReportRepository : IPostgreSqlGenericRepository<FinancialReport>
    {
        /// <summary>
        /// Get financial report by ticker, year and period
        /// </summary>
        Task<FinancialReport?> GetByTickerYearPeriodAsync(
            string ticker, 
            int year, 
            int period, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if financial report already exists
        /// </summary>
        Task<bool> ExistsAsync(
            string ticker, 
            int year, 
            int period, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get paginated financial reports by ticker
        /// </summary>
        Task<(IEnumerable<FinancialReport>, int)> GetByTickerPaginatedAsync(
            string ticker,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get paginated financial reports with filters
        /// </summary>
        Task<(List<FinancialReport>, int)> GetPaginatedAsync(
            int pageIndex,
            int pageSize,
            string? ticker = null,
            int? year = null,
            int? period = null,
            int? status = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a batch of financial reports for indicator data recalculation.
        /// </summary>
        Task<List<FinancialReport>> GetForIndicatorRecalculationAsync(
            string? ticker = null,
            int? year = null,
            int? period = null,
            bool onlyNullIndicatorData = true,
            int maxRecords = 2000,
            CancellationToken cancellationToken = default);
    }
}

