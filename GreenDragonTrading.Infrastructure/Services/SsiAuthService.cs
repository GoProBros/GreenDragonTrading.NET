using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants.SSI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text.Json;

namespace GreenDragonTrading.Infrastructure.Services
{
    public class SsiAuthService(
        HttpClient httpClient,
        ILogger<SsiAuthService> logger,
        IOptions<SsiApiOptionsV2> ssiApiOptions) : ISsiAuthService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<SsiAuthService> _logger = logger;
        private readonly SsiApiOptionsV2 _ssiOptions = ssiApiOptions.Value;

        private string? _accessToken;
        private DateTimeOffset _tokenExpirationTime = DateTimeOffset.MinValue;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private const int RefreshBufferSeconds = 600;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            if (!IsTokenExpired())
            {
                _logger.LogInformation("SSI Access token is still available do not need to get a new one.");
                return _accessToken!;
            }

            _logger.LogInformation("SSI Access token is expired are not existed. Getting a new one.");
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (!IsTokenExpired())
                {
                    return _accessToken!;
                }

                _accessToken = await FetchNewAccessTokenAsync(cancellationToken);
                _logger.LogInformation("Get new SSI access token successfully.");

                UpdateTokenExpiration(_accessToken);
                _logger.LogInformation("Update SSI access token expiration successfully.");
                return _accessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh SSI Access Token.");
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Check if access token is expired or 10 minutes before expired
        /// </summary>
        /// <returns>
        /// True: if token is expired or 10 minutes before expired
        /// False: if token is still availble
        /// </returns>
        private bool IsTokenExpired()
        {
            if (string.IsNullOrEmpty(_accessToken)) return true;

            return DateTimeOffset.UtcNow >= _tokenExpirationTime.AddSeconds(-RefreshBufferSeconds);
        }

        /// <summary>
        /// Update new exprired time for access token
        /// </summary>
        /// <param name="token">New access token get exp</param>
        private void UpdateTokenExpiration(string token)
        {
            _logger.LogInformation("Updating SSI access token expiration.");
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;

                if (long.TryParse(expClaim, out long expSeconds))
                {
                    _tokenExpirationTime = DateTimeOffset.FromUnixTimeSeconds(expSeconds);
                }
                else
                {
                    _tokenExpirationTime = DateTimeOffset.UtcNow.AddMinutes(10);
                    _logger.LogWarning("Cannot parse 'exp' claim from SSI Token.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing JWT Token.");
                _tokenExpirationTime = DateTimeOffset.MinValue;
            }
        }

        /// <summary>
        /// Get new access token from ssi service
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>New access token from ssi service</returns>
        private async Task<string> FetchNewAccessTokenAsync(CancellationToken cancellationToken)
        {
            var url = $"{_ssiOptions.FastConnectUrl}{SsiApiDefineV2.AccessToken}";

            var requestBody = new
            {
                consumerID = _ssiOptions.ConsumerID,
                consumerSecret = _ssiOptions.ConsumerSecret
            };

            _logger.LogInformation("Getting a new SSI access token from {Url}", url);

            var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<SingleResponse<AccessTokenResponse>>(content, _jsonOptions);

            if (result?.Data?.AccessToken == null)
            {
                throw new HttpRequestException($"SSI API returned OK but AccessToken is null. Message: {result?.Message}");
            }

            return result.Data.AccessToken;
        }
    }
}