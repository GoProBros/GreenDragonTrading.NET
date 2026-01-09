using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.Common.Utils;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants.SSI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace GreenDragonTrading.Infrastructure.Services
{
    /// <inheritdoc/>
    public class SsiServiceV2(
        HttpClient httpClient,
        ILogger<SsiServiceV2> logger,
        IOptions<SsiApiOptionsV2> ssiApiOptions,
        ISsiAuthService ssiAuthService) : ISsiServiceV2
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ISsiAuthService _ssiAuthService = ssiAuthService;
        private readonly ILogger<SsiServiceV2> _logger = logger;
        private readonly SsiApiOptionsV2 _ssiApiOptions = ssiApiOptions.Value;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <inheritdoc/>
        public async Task<(SecuritiesDetailsResponse result, int count)> FetchSecuritiesDetailsAsync(
            SecuritiesDetailsRequest requestQuery,
            CancellationToken cancellationToken = default)
        {
            string url = $"{_ssiApiOptions.FastConnectUrl}{SsiApiDefineV2.GetSecuritiesDetail}";

            string urlWithQuery = url + requestQuery.ToQueryString();

            _logger.LogInformation("Fetching SSI Securities from URL: {Url}", urlWithQuery);

            SecuritiesDetailsResponse result = await HandlerRequest<SecuritiesDetailsResponse>(urlWithQuery, cancellationToken);
            int actualCount = result.Data.Sum(item => item.RepeatedInfo.Count);
            _logger.LogInformation("Fetch successfully {Count} SSI Securities.", actualCount);

            return (result, actualCount);
        }

        /// <inheritdoc/>
        public async Task<(IntradayOhlcResponse result, int count)> FetchIntradayOhlcAsync(
            IntradayOhlcRequest requestQuery,
            CancellationToken cancellationToken = default)
        {
            string url = $"{_ssiApiOptions.FastConnectUrl}{SsiApiDefineV2.GetIntradayOhlc}";

            string urlWithQuery = url + requestQuery.ToQueryString();

            _logger.LogInformation("Fetching SSI Intraday OHLC from URL: {Url}", urlWithQuery);

            IntradayOhlcResponse result = await HandlerRequest<IntradayOhlcResponse>(urlWithQuery, cancellationToken);
            
            // Check for null data
            if (result?.Data == null)
            {
                _logger.LogWarning("SSI API returned null or empty data for Intraday OHLC");
                return (result ?? new IntradayOhlcResponse(), 0);
            }
            
            int actualCount = result.Data.Count;
            _logger.LogInformation("Fetch successfully {Count} SSI Intraday OHLC records.", actualCount);

            return (result, actualCount);
        }

        /// <summary>
        /// Handle the HTTP request to SSI API and deserialize the response.
        /// </summary>
        /// <typeparam name="TResponse">Response type for each API</typeparam>
        /// <param name="urlWithQuery">Query string for the API request</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        private async Task<TResponse> HandlerRequest<TResponse>(string urlWithQuery, CancellationToken cancellationToken)
            where TResponse : class, new()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, urlWithQuery);
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _ssiAuthService.GetAccessTokenAsync(cancellationToken));
                var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                return JsonSerializer.Deserialize<TResponse>(content, _jsonOptions) ?? new TResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching securities details from SSI API.");
                throw;
            }
        }
    }
}
