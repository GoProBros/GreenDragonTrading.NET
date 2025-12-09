namespace GreenDragonTrading.Application.Interfaces
{
    public interface ISsiAuthService
    {
        /// <summary>
        /// Get access token from ssi service
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>Access token provide from ssi</returns>
        Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    }
}
