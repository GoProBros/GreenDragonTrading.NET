using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Text.Json;

namespace GreenDragonTrading.Infrastructure.Services
{
    public class FinscService : IFinscService
    {
        private readonly IHttpClientFactory _httpClientFactorry;
        private readonly ILogger<FinscService> _logger;
        private readonly FinscApiOptions _finscOptions;
        public FinscService(IHttpClientFactory httpClientFactory, ILogger<FinscService> logger, IOptions<FinscApiOptions> finscOptions)
        {
            _httpClientFactorry = httpClientFactory;
            _logger = logger;
            _finscOptions = finscOptions.Value;
        }
        /// <inheritdoc/>
        public async Task<FinscStockResponse> GetStockDataAsync(FinscStockRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var url = $"{_finscOptions.FinscBaseUrl}{FinscApiDefine.GetStockData}";

                var queryParams = new Dictionary<string, string>
            {
                { "symbol", request.Symbol },
                { "resolution", request.Resolution },
                { "from", request.From.ToString() },
                { "to", request.To.ToString() }
            };

                var urlWithQuery = QueryHelpers.AddQueryString(url, queryParams);

                _logger.LogInformation("Calling Finsc API: {Url}", urlWithQuery);

                var httpClient = _httpClientFactorry.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(_finscOptions.TimeoutSeconds);

                var response = await httpClient.GetAsync(urlWithQuery);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Finsc API returned error status: {StatusCode}", response.StatusCode);

                    return new FinscStockResponse
                    {
                        Status = "error",
                        Symbol = request.Symbol
                    };
                }

                var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);

                _logger.LogDebug("Finsc API response: {JsonContent}", jsonContent);

                var stockData = JsonSerializer.Deserialize<FinscStockResponse>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (stockData == null)
                {
                    _logger.LogWarning("Failed to deserialize Finsc API response");
                    return new FinscStockResponse
                    {
                        Status = "error",
                        Symbol = request.Symbol
                    };
                }

                _logger.LogInformation(
                    "Successfully fetched {Count} data points from Finsc for {Symbol}",
                    stockData.Timestamps?.Length ?? 0,
                    request.Symbol);

                return stockData;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling Finsc API for {Symbol}", request.Symbol);
                throw new Exception($"Failed to fetch data from Finsc: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout calling Finsc API for {Symbol}", request.Symbol);
                throw new Exception("Request to Finsc API timed out", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling Finsc API for {Symbol}", request.Symbol);
                throw;
            }
        }
    }
}
