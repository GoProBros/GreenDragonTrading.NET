using GreenDragonTrading.Application.DTOs;

namespace GreenDragonTrading.Application.Interfaces
{
    public interface ISsiServiceV2
    {
        /// <summary>
        /// Fetch a list of securities from ssi api
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <param name="requestQuery">Request query</param>
        /// <returns>A list of securities from ssi api</returns>
        Task<(SecuritiesDetailsResponse result, int count)> FetchSecuritiesDetails(
            SecuritiesDetailsRequest requestQuery,
            CancellationToken cancellationToken = default);
    }
}
