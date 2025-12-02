using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace GreenDragonTrading.Infrastructure.Services
{
    public class SsiService(HttpClient httpClient, ILogger<SsiService> logger, IOptions<SsiApiOptions> ssiApiOptions) : ISsiService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<SsiService> _logger = logger;
        private readonly SsiApiOptions _ssiApiOptions = ssiApiOptions.Value;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<List<SsiSymbolDto>> FetchSymbolsListAsync(string exchange, CancellationToken cancellationToken = default)
        {
            var tasks = new[]
                {
                    FetchStocksListAsync(exchange, cancellationToken),
                    FetchETFsListAsync(exchange, cancellationToken),
                    FetchBondsListAsync(exchange, cancellationToken),
                    //FetchFuturesListAsync(cancellationToken)
                };

            await Task.WhenAll(tasks);

            return [.. tasks.SelectMany(t => t.Result)];
        }

        private async Task<List<SsiSymbolDto>> FetchStocksListAsync(string exchange, CancellationToken cancellationToken = default)
        {
            string url = $"{_ssiApiOptions.IBoardQuery}/stock/exchange/{exchange}";

            return await ParseSymbolsDataAsync(url, SymbolType.Stock, exchange, cancellationToken);
        }

        private async Task<List<SsiSymbolDto>> FetchETFsListAsync(string exchange, CancellationToken cancellationToken = default)
        {
            string url = $"{_ssiApiOptions.IBoardQuery}/stock/type/e/{exchange}";

            return await ParseSymbolsDataAsync(url, SymbolType.ETF, exchange, cancellationToken);
        }

        //private async Task<List<SsiSymbolDto>> FetchFuturesListAsync(CancellationToken cancellationToken = default)
        //{
        //    string url = $"{_ssiApiOptions.IBoardQuery}/stock/exchange/fu";

        //    return await ParseSymbolsDataAsync(url, SymbolType.Futures,null, cancellationToken);
        //}

        private async Task<List<SsiSymbolDto>> FetchBondsListAsync(string exchange, CancellationToken cancellationToken = default)
        {
            string url = $"{_ssiApiOptions.IBoardQuery}/stock/type/b/{exchange}bond";

            return await ParseSymbolsDataAsync(url, SymbolType.BOND, exchange, cancellationToken);
        }

        private async Task<List<SsiSymbolDto>> ParseSymbolsDataAsync(string url, SymbolType type , string? exchange, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Fetching SSI {Type} from URL: {Url}", type.ToString(), url);

                // Call API
                var response = await _httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                // Parse response
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

        public async Task<SsiSymbolDetailsDto?> FetchSymbolsDetailsAsync(string symbol, CancellationToken cancellationToken = default)
        {
            string url = $"{_ssiApiOptions.IBoardApi}/statistics/company/ssmi/company-profile?symbol={symbol}&language=vn";

            _logger.LogInformation("Fetching SSI symbol details from URL: {Url}", url);

            // Call API
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            // Parse response
            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<SsiApiResponse<SsiSymbolDetailsDto>>(jsonString, _jsonOptions);

            if (result?.IsSuccess != true)
            {
                _logger.LogError("Failed to fetch SSI symbols: {ErrorMessage}", result?.Message);
                return null;
            }

            _logger.LogInformation("Successfully fetched symbol {Symbol} details from SSI.", symbol);
            return result.Data;
        }

        public async Task<List<SsiIndustryDto>> FetchIndustryListAsync(CancellationToken cancellationToken = default)
        {
            string url = $"{_ssiApiOptions.IBoardApi}/statistics/company/sectors-data-v2";

            _logger.LogInformation("Fetching SSI industries from URL: {Url}", url);

            // Call API
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            // Parse response
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
