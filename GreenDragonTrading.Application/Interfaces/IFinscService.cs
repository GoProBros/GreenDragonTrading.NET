using GreenDragonTrading.Application.DTOs;

namespace GreenDragonTrading.Application.Interfaces
{
    /// <summary>
    /// Service to interact with Finsc Api for stock chart data
    /// </summary>
    public interface IFinscService
    {
        /// <summary>
        /// Fetch OHLCV chart data from Finsc API
        /// </summary>
        /// <param name="request">Request parameters (symbol, resolution, from, to)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>OHLCV data in parallel arrays format</returns>
        Task<FinscStockResponse> GetStockDataAsync(FinscStockRequest request, CancellationToken cancellationToken = default);
    }
}
