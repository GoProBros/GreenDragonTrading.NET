using Google.Apis.Auth;
using GreenDragonTrading.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Infrastructure.Services
{
    /// <summary>
    /// Verifies Google ID tokens using Google.Apis.Auth.
    /// </summary>
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly string _clientId;
        private readonly ILogger<GoogleAuthService> _logger;

        public GoogleAuthService(IConfiguration configuration, ILogger<GoogleAuthService> logger)
        {
            _clientId = configuration["Google:ClientId"]
                ?? throw new InvalidOperationException("Google:ClientId is not configured.");
            _logger = logger;
        }

        public async Task<GoogleUserInfo> VerifyIdTokenAsync(string idToken, CancellationToken cancellationToken = default)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _clientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            _logger.LogInformation("Google token verified for email: {Email}", payload.Email);

            return new GoogleUserInfo
            {
                Email = payload.Email,
                Name = payload.Name ?? payload.Email,
                PictureUrl = payload.Picture,
                GoogleId = payload.Subject,
            };
        }
    }
}
