using GreenDragonTrading.Application.DTOs;

namespace GreenDragonTrading.Application.Interfaces
{
    public interface ISsiService
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
        /// <returns>A list of <see cref="SsiIndustryDto"/> containing industry/sector information from SSI.</returns>
        Task<List<SsiIndustryDto>> FetchIndustryListAsync(CancellationToken cancellationToken = default);
    }
}
