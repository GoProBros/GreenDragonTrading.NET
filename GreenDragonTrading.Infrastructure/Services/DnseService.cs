using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants.DNSE;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Infrastructure.Services;

/// <summary>
/// Service for integrating with DNSE API
/// </summary>
public class DnseService : IDnseService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DnseService> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public DnseService(HttpClient httpClient, ILogger<DnseService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<DnseFinancialReportResponse?> GetFinancialReportDetailsAsync(
        string ticker,
        string reportCode,
        string cycleType,
        int cycleNumber,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(ticker))
                throw new ArgumentException("Ticker cannot be null or empty", nameof(ticker));

            if (string.IsNullOrWhiteSpace(reportCode))
                throw new ArgumentException("Report code cannot be null or empty", nameof(reportCode));

            if (cycleType != DnseConstants.CycleType.QUARTERLY && cycleType != DnseConstants.CycleType.YEARLY)
                throw new ArgumentException($"Invalid cycle type. Must be '{DnseConstants.CycleType.QUARTERLY}' or '{DnseConstants.CycleType.YEARLY}'", nameof(cycleType));

            if (cycleNumber != 5 && cycleNumber != 10)
                throw new ArgumentException("Cycle number must be 5 or 10", nameof(cycleNumber));

            // Build URL with query parameters
            var url = $"{DnseConstants.FINANCIAL_REPORT_ENDPOINT}?symbol={ticker}&code={reportCode}&cycleType={cycleType}&cycleNumber={cycleNumber}";

            _logger.LogInformation("Fetching financial report from DNSE: {Ticker}, {Code}, {CycleType}, {CycleNumber}", 
                ticker, reportCode, cycleType, cycleNumber);

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("DNSE API returned error status {StatusCode} for {Ticker} - {Code}", 
                    response.StatusCode, ticker, reportCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("DNSE API returned empty response for {Ticker} - {Code}", ticker, reportCode);
                return null;
            }

            var result = JsonSerializer.Deserialize<DnseFinancialReportResponse>(content, _jsonOptions);

            if (result == null)
            {
                _logger.LogWarning("Failed to deserialize DNSE response for {Ticker} - {Code}", ticker, reportCode);
                return null;
            }

            _logger.LogDebug("Successfully fetched financial report from DNSE for {Ticker} - {Code}: {PeriodCount} periods", 
                ticker, reportCode, result.X.Count);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error when fetching financial report from DNSE for {Ticker} - {Code}", ticker, reportCode);
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error for DNSE response: {Ticker} - {Code}", ticker, reportCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when fetching financial report from DNSE for {Ticker} - {Code}", ticker, reportCode);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<DnseCorporateActionsResponse?> GetCorporateActionsHistoryAsync(
        string ticker,
        int page,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ticker))
        {
            throw new ArgumentException("Ticker cannot be null or empty", nameof(ticker));
        }

        if (page <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Page must be greater than 0");
        }

        var url = $"{DnseConstants.CORPORATE_ACTIONS_HISTORY_ENDPOINT}?symbol={ticker}&page={page}";
        return await FetchCorporateActionsAsync(url, $"history:{ticker}", cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DnseCorporateActionsResponse?> GetCorporateActionsUpcomingAsync(
        int page,
        CancellationToken cancellationToken = default)
    {
        if (page <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Page must be greater than 0");
        }

        var url = $"{DnseConstants.CORPORATE_ACTIONS_UPCOMING_ENDPOINT}?page={page}";
        return await FetchCorporateActionsAsync(url, "upcoming", cancellationToken);
    }

    private async Task<DnseCorporateActionsResponse?> FetchCorporateActionsAsync(
        string url,
        string logScope,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching DNSE corporate actions ({Scope})", logScope);

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("DNSE corporate actions ({Scope}) returned status {StatusCode}", logScope, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("DNSE corporate actions ({Scope}) returned empty response", logScope);
                return null;
            }

            var result = JsonSerializer.Deserialize<DnseCorporateActionsResponse>(content, _jsonOptions);

            if (result == null)
            {
                _logger.LogWarning("Failed to deserialize DNSE corporate actions ({Scope})", logScope);
                return null;
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error when fetching DNSE corporate actions ({Scope})", logScope);
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error for DNSE corporate actions ({Scope})", logScope);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when fetching DNSE corporate actions ({Scope})", logScope);
            return null;
        }
    }
}
