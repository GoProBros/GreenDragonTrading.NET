using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace GreenDragonTrading.Infrastructure.Services
{
    public class VndService(HttpClient httpClient, ILogger<VndService> logger, IOptions<VndApiOptions> vndApiOptions) : IVndService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<VndService> _logger = logger;
        private readonly VndApiOptions _vndApiOptions = vndApiOptions.Value;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<List<VndIndustryResponseDto>> FetchIndustriesListAsync(string level, string pageSize, CancellationToken cancellationToken = default)
        {
            string url = $"{_vndApiOptions.BaseUrl}/v4/industry_classification?q=industryLevel:{level}&size={pageSize}&page=1";

            _logger.LogInformation("Fetching VND industries from URL: {Url}", url);

            // Call API
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            // Parse response
            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<VndPaginatedApiResponse<VndIndustryResponseDto>>(jsonString, _jsonOptions);

            _logger.LogInformation("Successfully fetched {Count} industry level {Level} from VND.", result?.Data?.Count ?? 0, level);
            return result?.Data ?? [];
        }
    }
}
