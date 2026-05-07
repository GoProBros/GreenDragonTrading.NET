using GreenDragonTrading.Application.DTOs;

namespace GreenDragonTrading.Application.Interfaces
{
    /// <summary>
    /// Provides Google ID token verification.
    /// </summary>
    public interface IGoogleAuthService
    {
        /// <summary>
        /// Verifies the Google ID token and returns the payload if valid.
        /// </summary>
        /// <param name="idToken">Google ID token from the frontend.</param>
        /// <returns>Verified payload containing email, name, picture, etc.</returns>
        Task<GoogleUserInfo> VerifyIdTokenAsync(string idToken, CancellationToken cancellationToken = default);
    }
}
