using GreenDragonTrading.Application.DTOs;

namespace GreenDragonTrading.Application.Interfaces
{
    /// <summary>
    /// Service for fetching symbols/sectors from SSI V2 REST API.
    /// </summary>
    public interface ISsiServiceV2
    {
        /// <summary>
        /// Fetch a list of securities from ssi api
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <param name="requestQuery">Request query</param>
        /// <returns>A list of securities from ssi api</returns>
        Task<(SecuritiesDetailsResponse result, int count)> FetchSecuritiesDetailsAsync(
            SecuritiesDetailsRequest requestQuery,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetch intraday OHLC data from ssi api
        /// </summary>
        /// <param name="requestQuery">Request query</param>
        /// <param name="cancellationToken">Cancellation token for the request</param>
        /// <returns>A list of intraday OHLC data from ssi api</returns>
        Task<(IntradayOhlcResponse result, int count)> FetchIntradayOhlcAsync(
            IntradayOhlcRequest requestQuery,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetch daily OHLC data from ssi api
        /// </summary>
        /// <param name="requestQuery">Request query</param>
        /// <param name="cancellationToken">Cancellation token for the request</param>
        /// <returns>A list of daily OHLC data from ssi api</returns>
        Task<(DailyOhlcResponse result, int count)> FetchDailyOhlcAsync(
            DailyOhlcRequest requestQuery,
            CancellationToken cancellationToken = default);
    }
}
