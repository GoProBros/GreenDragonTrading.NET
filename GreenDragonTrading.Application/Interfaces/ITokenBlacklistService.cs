namespace GreenDragonTrading.Application.Interfaces
{
    /// <summary>
    /// Service for managing access token blacklist
    /// </summary>
    public interface ITokenBlacklistService
    {
        /// <summary>
        /// Blacklist an access token until its expiration
        /// </summary>
        /// <param name="jti">JWT ID (JTI claim)</param>
        /// <param name="expiresAt">Token expiration time</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task BlacklistTokenAsync(string jti, DateTime expiresAt, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if a token is blacklisted
        /// </summary>
        /// <param name="jti">JWT ID (JTI claim)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if token is blacklisted</returns>
        Task<bool> IsTokenBlacklistedAsync(string jti, CancellationToken cancellationToken = default);
    }
}
