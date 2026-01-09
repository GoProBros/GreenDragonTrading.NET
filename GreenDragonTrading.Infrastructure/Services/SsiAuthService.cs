using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants.SSI;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text.Json;

namespace GreenDragonTrading.Infrastructure.Services
{
    /// <inheritdoc/>
    public class SsiAuthService(
        HttpClient httpClient,
        ILogger<SsiAuthService> logger,
        IOptions<SsiApiOptionsV2> ssiApiOptions,
        IMemoryCache cache) : ISsiAuthService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<SsiAuthService> _logger = logger;
        private readonly SsiApiOptionsV2 _ssiOptions = ssiApiOptions.Value;
        private readonly IMemoryCache _cache = cache;

        private const string CacheKey = "SSI_ACCESS_TOKEN";
        private const int RefreshBufferSeconds = 600; // 10 minutes buffer

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <inheritdoc/>
        public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            var token = await _cache.GetOrCreateAsync(CacheKey, async entry =>
            {
                _logger.LogInformation("SSI Access token in cache is missing or expired. Fetching a new one...");

                var newToken = await FetchNewAccessTokenAsync(cancellationToken);
                var expirationTime = ParseTokenExpiration(newToken);

                entry.AbsoluteExpiration = expirationTime.AddSeconds(-RefreshBufferSeconds);

                _logger.LogInformation("New SSI Access Token cached. Cache expires at: {Expiry}", entry.AbsoluteExpiration);

                return newToken;
            });

            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("Failed to retrieve or cache SSI Access Token.");
            }

             _logger.LogInformation("SSI Access token is available in cache.");
            return token;
        }

        /// <summary>
        /// Get new access token from SSI service API.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>New raw access token string.</returns>
        /// <exception cref="HttpRequestException">Thrown when API call fails or returns empty token.</exception>
        private async Task<string> FetchNewAccessTokenAsync(CancellationToken cancellationToken)
        {
            var url = $"{_ssiOptions.FastConnectUrl}{SsiApiDefineV2.AccessToken}";

            var requestBody = new
            {
                consumerID = _ssiOptions.ConsumerID,
                consumerSecret = _ssiOptions.ConsumerSecret
            };

            _logger.LogInformation("Requesting new SSI access token from {Url}", url);

            var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<SingleResponse<AccessTokenResponse>>(content, _jsonOptions);

            if (string.IsNullOrEmpty(result?.Data?.AccessToken))
            {
                throw new HttpRequestException($"SSI API returned OK but AccessToken is null. Message: {result?.Message}");
            }

            return result.Data.AccessToken;
        }

        /// <summary>
        /// Parse the JWT token to extract the 'exp' (expiration) claim.
        /// </summary>
        /// <param name="token">The JWT Access Token.</param>
        /// <returns>DateTimeOffset representing the expiration time.</returns>
        private DateTimeOffset ParseTokenExpiration(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;

                if (long.TryParse(expClaim, out long expSeconds))
                {
                    return DateTimeOffset.FromUnixTimeSeconds(expSeconds);
                }

                _logger.LogWarning("Cannot parse 'exp' claim from SSI Token. Using default 1 hour expiration.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing JWT Token structure.");
            }

            return DateTimeOffset.UtcNow.AddHours(1);
        }
    }
}