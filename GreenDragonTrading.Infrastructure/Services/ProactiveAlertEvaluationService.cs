using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace GreenDragonTrading.Infrastructure.Services
{
    /// <summary>
    /// HTTP client service for proactive signal-level AI evaluation endpoint.
    /// </summary>
    public class ProactiveAlertEvaluationService(
        HttpClient httpClient,
        IOptions<AiEngineOptions> options,
        ILogger<ProactiveAlertEvaluationService> logger) : IProactiveAlertEvaluationService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly AiEngineOptions _options = options.Value;
        private readonly ILogger<ProactiveAlertEvaluationService> _logger = logger;

        public async Task<ProactiveAlertEvaluationClientResult> EvaluateAsync(
            ProactiveAiEvaluationJobDto job,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    NormalizeEndpoint(_options.ProactiveAlertEvaluationEndpoint))
                {
                    Content = JsonContent.Create(job)
                };

                var response = await _httpClient.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<ProactiveAlertEvaluationResponse>(cancellationToken: cancellationToken);
                    if (data == null || string.IsNullOrWhiteSpace(data.UserMessage))
                    {
                        return ProactiveAlertEvaluationClientResult.Failure("AI service returned empty evaluation payload.", "EMPTY_RESPONSE");
                    }

                    return new ProactiveAlertEvaluationClientResult
                    {
                        Success = true,
                        Data = data,
                    };
                }

                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var (errorCode, errorMessage) = TryParseError(errorBody);

                if ((int)response.StatusCode == StatusCodes.Status400BadRequest)
                {
                    return ProactiveAlertEvaluationClientResult.Failure(
                        errorMessage ?? "Invalid proactive evaluation request.",
                        errorCode ?? "INVALID_REQUEST",
                        isValidationError: true);
                }

                if ((int)response.StatusCode == StatusCodes.Status409Conflict)
                {
                    return ProactiveAlertEvaluationClientResult.Failure(
                        errorMessage ?? "Duplicate proactive job id with mismatched payload.",
                        errorCode ?? "CONFLICT",
                        isConflict: true);
                }

                return ProactiveAlertEvaluationClientResult.Failure(
                    errorMessage ?? $"AI service returned {(int)response.StatusCode}.",
                    errorCode);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Proactive evaluation service is unavailable for job {JobId}", job.JobId);
                return ProactiveAlertEvaluationClientResult.Failure("AI service is unavailable.", "SERVICE_UNAVAILABLE");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "Proactive evaluation request timed out for job {JobId}", job.JobId);
                return ProactiveAlertEvaluationClientResult.Failure("AI evaluation request timed out.", "TIMEOUT");
            }
        }

        private static string NormalizeEndpoint(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return "/";
            }

            return endpoint.StartsWith('/') ? endpoint : $"/{endpoint}";
        }

        private static (string? ErrorCode, string? ErrorMessage) TryParseError(string errorBody)
        {
            if (string.IsNullOrWhiteSpace(errorBody))
            {
                return (null, null);
            }

            try
            {
                var parsed = JsonSerializer.Deserialize<ProactiveAlertErrorResponse>(errorBody);
                return (parsed?.Code, parsed?.Message);
            }
            catch (JsonException)
            {
                return (null, errorBody);
            }
        }

        private sealed class ProactiveAlertErrorResponse
        {
            public string? Code { get; set; }
            public string? Message { get; set; }
        }
    }
}
