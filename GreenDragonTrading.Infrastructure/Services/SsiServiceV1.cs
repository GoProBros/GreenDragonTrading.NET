using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace GreenDragonTrading.Infrastructure.Services
{
    public class SsiServiceV1(HttpClient httpClient, ILogger<SsiServiceV1> logger, IOptions<SsiApiOptionsV1> ssiApiOptions) : ISsiServiceV1
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<SsiServiceV1> _logger = logger;
        private readonly SsiApiOptionsV1 _ssiApiOptions = ssiApiOptions.Value;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Fetches the complete list of symbols (stocks, ETFs, bonds) from SSI API for a specific exchange.
        /// This method aggregates results from multiple symbol type endpoints in parallel.
        /// </summary>
        /// <param name="exchange">The exchange code (e.g., HSX, HNX, UPCOM).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A combined list of <see cref="SsiSymbolDto"/> from all symbol types.</returns>
        public async Task<List<SsiSymbolDto>> FetchSymbolsListAsync(string exchange, CancellationToken cancellationToken = default)
        {
            var tasks = new[]
                {
                    FetchStocksListAsync(exchange, cancellationToken),
                    FetchETFsListAsync(exchange, cancellationToken),
                    FetchBondsListAsync(exchange, cancellationToken),
                };

            await Task.WhenAll(tasks);

            return [.. tasks.SelectMany(t => t.Result)];
        }

        /// <summary>
        /// Fetches the list of stock symbols from SSI API for a specific exchange.
        /// </summary>
        /// <param name="exchange">The exchange code (e.g., HSX, HNX, UPCOM).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A list of stock symbols from SSI.</returns>
        private async Task<List<SsiSymbolDto>> FetchStocksListAsync(string exchange, CancellationToken cancellationToken = default)
        {
            string url = $"{_ssiApiOptions.IBoardQuery}/stock/exchange/{exchange}";

            return await ParseSymbolsDataAsync(url, SymbolType.Stock, exchange, cancellationToken);
        }

        /// <summary>
        /// Fetches the list of ETF symbols from SSI API for a specific exchange.
        /// </summary>
        /// <param name="exchange">The exchange code (e.g., HSX, HNX, UPCOM).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A list of ETF symbols from SSI.</returns>
        private async Task<List<SsiSymbolDto>> FetchETFsListAsync(string exchange, CancellationToken cancellationToken = default)
        {
            string url = $"{_ssiApiOptions.IBoardQuery}/stock/type/e/{exchange}";

            return await ParseSymbolsDataAsync(url, SymbolType.ETF, exchange, cancellationToken);
        }

        /// <summary>
        /// Fetches the list of bond symbols from SSI API for a specific exchange.
        /// </summary>
        /// <param name="exchange">The exchange code (e.g., HSX, HNX, UPCOM).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A list of bond symbols from SSI.</returns>
        private async Task<List<SsiSymbolDto>> FetchBondsListAsync(string exchange, CancellationToken cancellationToken = default)
        {
            string url = $"{_ssiApiOptions.IBoardQuery}/stock/type/b/{exchange}bond";

            return await ParseSymbolsDataAsync(url, SymbolType.BOND, exchange, cancellationToken);
        }

        /// <summary>
        /// Parses symbol data from SSI API response for a given URL and symbol type.
        /// Handles HTTP request, response parsing, and error logging.
        /// </summary>
        /// <param name="url">The SSI API endpoint URL to fetch data from.</param>
        /// <param name="type">The type of symbols being fetched (Stock, ETF, Bond, Futures).</param>
        /// <param name="exchange">The exchange code, or null for futures.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A list of parsed symbol DTOs, or an empty list on failure.</returns>
        private async Task<List<SsiSymbolDto>> ParseSymbolsDataAsync(string url, SymbolType type , string? exchange, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Fetching SSI {Type} from URL: {Url}", type.ToString(), url);

                var response = await _httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<SsiQueryResponse<List<SsiSymbolDto>>>(jsonString, _jsonOptions);

                if (result?.IsSuccess != true)
                {
                    _logger.LogError("Failed to fetch SSI {Type}: {ErrorMessage}", type.ToString(), result?.Message);
                    return [];
                }

                if (type == SymbolType.Futures)
                {
                    _logger.LogInformation("Successfully fetched {Count} {Type} from SSI.", result.Data?.Count ?? 0, type.ToString());
                }
                else
                {
                    _logger.LogInformation("Successfully fetched {Count} {Type} of {Exchange} from SSI.", result.Data?.Count ?? 0, type.ToString(), exchange);
                }

                return result.Data ?? [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while fetching SSI {Type} from URL: {Url}", type.ToString(), url);
                return [];
            }
        }

        /// <summary>
        /// Fetches detailed company profile information for a specific symbol from SSI API.
        /// Includes company name, industry, financial data, and other company-specific details.
        /// </summary>
        /// <param name="symbol">The stock symbol/ticker to fetch details for.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A <see cref="SsiSymbolDetailsDto"/> with company details, or null if fetch fails.</returns>
        public async Task<SsiSymbolDetailsDto?> FetchSymbolsDetailsAsync(string symbol, CancellationToken cancellationToken = default)
        {
            string url = $"{_ssiApiOptions.IBoardApi}/statistics/company/ssmi/company-profile?symbol={symbol}&language=vn";

            _logger.LogInformation("Fetching SSI symbol details from URL: {Url}", url);

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<SsiApiResponseV1<SsiSymbolDetailsDto>>(jsonString, _jsonOptions);

            if (result?.IsSuccess != true)
            {
                _logger.LogError("Failed to fetch SSI symbols: {ErrorMessage}", result?.Message);
                return null;
            }

            _logger.LogInformation("Successfully fetched symbol {Symbol} details from SSI.", symbol);
            return result.Data;
        }

        /// <summary>
        /// Fetches the complete list of industry sectors from SSI API.
        /// Retrieves all sector levels (1-4) with Vietnamese and English names.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A list of <see cref="SsiIndustryDto"/> containing all industry sectors.</returns>
        public async Task<List<SsiIndustryDto>> FetchIndustryListAsync(int? level, CancellationToken cancellationToken = default)
        {
            string url = $"{_ssiApiOptions.IBoardApi}/statistics/company/sectors-data-v2";
            if (level.HasValue)
            {
                url = $"{_ssiApiOptions.IBoardApi}/statistics/company/sectors-data-v2?level={level}";
            }

            _logger.LogInformation("Fetching SSI industries from URL: {Url}", url);

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<SsiQueryResponse<List<SsiIndustryDto>>>(jsonString, _jsonOptions);

            if (result?.IsSuccess != true)
            {
                _logger.LogError("Failed to fetch SSI industries: {ErrorMessage}", result?.Message);
                return [];
            }

            _logger.LogInformation("Successfully fetched {Count} industries from SSI.", result.Data?.Count ?? 0);
            return result.Data ?? [];
        }
    }
}
