using GreenDragonTrading.Application.DTOs;

namespace GreenDragonTrading.Application.Interfaces
{
    /// <summary>
    /// Service for fetching symbols/sectors from SSI V1 REST API.
    /// </summary>
    public interface ISsiServiceV1
    {
        /// <summary>
        /// Fetches the list of symbols (stocks, ETFs, bonds) from SSI API for a specific exchange.
        /// </summary>
        /// <param name="exchange">The exchange code (e.g., HSX, HNX, UPCOM).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A list of <see cref="SsiSymbolDto"/> containing symbol information from SSI.</returns>
        Task<List<SsiSymbolDto>> FetchSymbolsListAsync(string exchange, CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetches detailed information for a specific symbol from SSI API.
        /// </summary>
        /// <param name="symbol">The stock symbol/ticker to fetch details for.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A <see cref="SsiSymbolDetailsDto"/> containing detailed symbol information, or null if not found.</returns>
        Task<SsiSymbolDetailsDto?> FetchSymbolsDetailsAsync(string symbol, CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetches the list of all industry sectors from SSI API.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <param name="level">The industry level to filter by (e.g., 1, 2, 3). If null, fetches all levels.</param>
        /// <returns>A list of <see cref="SsiIndustryDto"/> containing industry/sector information from SSI.</returns>
        Task<List<SsiIndustryDto>> FetchIndustryListAsync(int? level, CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetches the list of constituent symbols for a given market index from SSI API.
        /// </summary>
        /// <param name="indexCode">The index code (e.g., "VN30", "HNX30").</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A list of <see cref="SsiSymbolDto"/> that are constituents of the specified index.</returns>
        Task<List<SsiSymbolDto>> FetchIndexConstituentsAsync(string indexCode, CancellationToken cancellationToken = default);
    }
}
