using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace GreenDragonTrading.Infrastructure.Services
{
    /// <summary>
    /// Service for communicating with the GreenDragonTrading AI Engine
    /// </summary>
    public class AiChatService : IAiChatService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AiChatService> _logger;
        private readonly AiEngineOptions _options;

        public AiChatService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptions<AiEngineOptions> options,
            ILogger<AiChatService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _options = options.Value;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<AiChatSubmissionResult> SubmitMessageAsync(
            string conversationId,
            string message,
            AiChatContext? context = null,
            CancellationToken cancellationToken = default)
        {
            var request = new AiChatRequest
            {
                ConversationId = conversationId,
                Message = message,
                Context = context,
                ExecutionMode = "auto"
            };

            _logger.LogDebug("Submitting message to AI Engine for conversation {ConversationId}", conversationId);

            HttpResponseMessage response;
            try
            {
                using var requestMessage = CreatePostRequest(NormalizeEndpoint(_options.ChatEndpoint), request);
                response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "AI Engine is unavailable for conversation {ConversationId}", conversationId);
                return new AiChatSubmissionResult
                {
                    Success = false,
                    Accepted = false,
                    Error = "AI Engine is currently unavailable. Please try again later."
                };
            }

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var syncBody = await response.Content.ReadFromJsonAsync<AiChatResponse>(cancellationToken: cancellationToken);
                if (syncBody == null)
                {
                    _logger.LogWarning("AI Engine returned null sync response for conversation {ConversationId}", conversationId);
                    return new AiChatSubmissionResult
                    {
                        Success = false,
                        Accepted = false,
                        Error = "AI Engine returned empty sync response"
                    };
                }

                return new AiChatSubmissionResult
                {
                    Success = syncBody.Success,
                    Accepted = false,
                    Response = syncBody,
                    Error = syncBody.Error
                };
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                var accepted = await response.Content.ReadFromJsonAsync<AiChatAcceptedResponse>(cancellationToken: cancellationToken);
                if (accepted == null)
                {
                    _logger.LogWarning("AI Engine returned null accepted response for conversation {ConversationId}", conversationId);
                    return new AiChatSubmissionResult
                    {
                        Success = false,
                        Accepted = false,
                        Error = "AI Engine returned invalid accepted response"
                    };
                }

                return new AiChatSubmissionResult
                {
                    Success = accepted.Success,
                    Accepted = true,
                    AcceptedResponse = accepted
                };
            }

            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "AI Engine returned {StatusCode} for conversation {ConversationId}: {ErrorBody}",
                response.StatusCode,
                conversationId,
                errorBody);

            return new AiChatSubmissionResult
            {
                Success = false,
                Accepted = false,
                Error = $"AI Engine returned {(int)response.StatusCode}: {errorBody}"
            };
        }

        /// <inheritdoc />
        public async Task<AiChatJobStatusResponse?> GetJobStatusAsync(
            string pollUrl,
            CancellationToken cancellationToken = default)
        {
            var normalizedPollUrl = NormalizePollUrl(pollUrl);

            HttpResponseMessage pollRes;
            try
            {
                using var pollReq = new HttpRequestMessage(HttpMethod.Get, normalizedPollUrl);
                AttachAuthorizationHeader(pollReq);
                pollRes = await _httpClient.SendAsync(pollReq, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Polling AI job failed. PollUrl={PollUrl}", normalizedPollUrl);
                return new AiChatJobStatusResponse
                {
                    Success = false,
                    Accepted = false,
                    Status = "failed",
                    Error = ex.Message
                };
            }

            if (pollRes.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new AiChatJobStatusResponse
                {
                    Success = false,
                    Accepted = false,
                    Status = "failed",
                    Error = "Async job not found or expired."
                };
            }

            if (pollRes.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                var pendingStatus = await pollRes.Content.ReadFromJsonAsync<AiChatJobStatusResponse>(cancellationToken: cancellationToken);
                return pendingStatus ?? new AiChatJobStatusResponse
                {
                    Success = true,
                    Accepted = true,
                    Status = "running"
                };
            }

            if (!pollRes.IsSuccessStatusCode)
            {
                var err = await pollRes.Content.ReadAsStringAsync(cancellationToken);
                return new AiChatJobStatusResponse
                {
                    Success = false,
                    Accepted = false,
                    Status = "failed",
                    Error = $"Unexpected poll status {pollRes.StatusCode}: {err}"
                };
            }

            return await pollRes.Content.ReadFromJsonAsync<AiChatJobStatusResponse>(cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<AiChatResponse> SendMessageAsync(
            string conversationId,
            string message,
            AiChatContext? context = null,
            CancellationToken cancellationToken = default)
        {
            var submitResult = await SubmitMessageAsync(conversationId, message, context, cancellationToken);
            if (!submitResult.Success)
            {
                return new AiChatResponse
                {
                    Success = false,
                    ConversationId = conversationId,
                    Error = submitResult.Error ?? "AI Engine returned an unknown error."
                };
            }

            if (!submitResult.Accepted)
            {
                if (submitResult.Response == null)
                {
                    return new AiChatResponse
                    {
                        Success = false,
                        ConversationId = conversationId,
                        Error = "AI Engine returned empty sync response"
                    };
                }

                submitResult.Response.ConversationId ??= conversationId;
                return submitResult.Response;
            }

            var accepted = submitResult.AcceptedResponse;
            if (accepted == null)
            {
                return new AiChatResponse
                {
                    Success = false,
                    ConversationId = conversationId,
                    Error = "AI Engine returned invalid accepted response"
                };
            }

            var pollPath = string.IsNullOrWhiteSpace(accepted.PollUrl)
                ? $"/api/chat/jobs/{accepted.JobId}"
                : accepted.PollUrl;

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                var pollStatus = await GetJobStatusAsync(pollPath, cancellationToken);
                if (pollStatus == null)
                {
                    return new AiChatResponse
                    {
                        Success = false,
                        ConversationId = conversationId,
                        Error = "Invalid poll response"
                    };
                }

                if (string.Equals(pollStatus.Status, "queued", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(pollStatus.Status, "running", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (string.Equals(pollStatus.Status, "completed", StringComparison.OrdinalIgnoreCase))
                {
                    if (pollStatus.Result != null)
                    {
                        pollStatus.Result.ConversationId ??= conversationId;
                        return pollStatus.Result;
                    }

                    return new AiChatResponse
                    {
                        Success = false,
                        ConversationId = conversationId,
                        Error = "Empty completed result"
                    };
                }

                return new AiChatResponse
                {
                    Success = false,
                    ConversationId = conversationId,
                    Error = $"Async chat failed: {pollStatus.Error ?? "Unknown error"}"
                };
            }

            return new AiChatResponse
            {
                Success = false,
                ConversationId = conversationId,
                Error = "Polling cancelled"
            };
        }

        /// <inheritdoc />
        public async Task<AiConvSummaryResponse> UpdateSummaryAsync(
            string conversationId,
            string? existingSummary,
            List<AiMessageInput> newMessages,
            CancellationToken cancellationToken = default)
        {
            var request = new AiConvSummaryRequest
            {
                ConversationId = conversationId,
                ExistingSummary = existingSummary,
                NewMessages = newMessages,
                Timestamp = DateTimeOffset.UtcNow
            };

            _logger.LogDebug("Updating conversation summary for {ConversationId} with {Count} messages", conversationId, newMessages.Count);

            HttpResponseMessage response;
            try
            {
                using var requestMessage = CreatePostRequest(NormalizeEndpoint(_options.ConversationSummaryEndpoint), request);
                response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "AI Engine is unavailable when updating summary for {ConversationId}", conversationId);
                return new AiConvSummaryResponse { Success = false, ConversationId = conversationId, Error = "AI Engine unavailable" };
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Conv-summary returned {StatusCode} for {ConversationId}: {ErrorBody}", response.StatusCode, conversationId, errorBody);
                return new AiConvSummaryResponse { Success = false, ConversationId = conversationId, Error = errorBody };
            }

            var summaryResponse = await response.Content.ReadFromJsonAsync<AiConvSummaryResponse>(cancellationToken: cancellationToken);
            if (summaryResponse == null)
            {
                return new AiConvSummaryResponse { Success = false, ConversationId = conversationId, Error = "Empty response from conv-summary" };
            }

            _logger.LogDebug("Summary updated for {ConversationId}, processed {Count} messages", conversationId, summaryResponse.ProcessedMessages);
            return summaryResponse;
        }

        /// <inheritdoc />
        public async Task<AiNewsSummarizationResponse?> SummarizeNewsAsync(
            string title,
            string content,
            CancellationToken cancellationToken = default)
        {
            var request = new AiNewsSummarizationRequest
            {
                Title = title,
                Content = content
            };

            HttpResponseMessage response;
            try
            {
                using var requestMessage = CreatePostRequest(NormalizeEndpoint(_options.NewsSummarizationEndpoint), request);
                response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "AI Engine is unavailable when summarizing news");
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("News summarization returned {StatusCode}: {ErrorBody}", response.StatusCode, errorBody);
                return null;
            }

            var summaryResponse = await response.Content.ReadFromJsonAsync<AiNewsSummarizationResponse>(cancellationToken: cancellationToken);
            if (summaryResponse == null)
            {
                _logger.LogWarning("AI Engine returned empty response for news summarization");
                return null;
            }

            return summaryResponse;
        }

        /// <inheritdoc />
        public async Task<AiChatSuggestionsResponse?> GetSuggestionsAsync(
            CancellationToken cancellationToken = default)
        {
            var endpoint = NormalizeEndpoint(_options.SuggestionsEndpoint);

            HttpResponseMessage response;
            try
            {
                using var requestMessage = new HttpRequestMessage(HttpMethod.Get, endpoint);
                AttachAuthorizationHeader(requestMessage);
                response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "AI Engine is unavailable when fetching suggestions");
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Suggestions endpoint returned {StatusCode}: {ErrorBody}", response.StatusCode, errorBody);
                return null;
            }

            var suggestionsResponse = await response.Content.ReadFromJsonAsync<AiChatSuggestionsResponse>(cancellationToken: cancellationToken);
            if (suggestionsResponse == null)
            {
                _logger.LogWarning("AI Engine returned empty response for suggestions");
                return null;
            }

            _logger.LogDebug("Retrieved {Count} suggestions from AI Engine", suggestionsResponse.Questions.Count);
            return suggestionsResponse;
        }

        private HttpRequestMessage CreatePostRequest(string endpoint, object payload)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = JsonContent.Create(payload)
            };

            AttachAuthorizationHeader(requestMessage);
            return requestMessage;
        }

        private void AttachAuthorizationHeader(HttpRequestMessage requestMessage)
        {
            var incomingAuthHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(incomingAuthHeader)
                && incomingAuthHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                requestMessage.Headers.TryAddWithoutValidation("Authorization", incomingAuthHeader);
            }
        }

        private static string NormalizeEndpoint(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return "/";
            }

            return endpoint.StartsWith("/") ? endpoint : $"/{endpoint}";
        }

        private static string NormalizePollUrl(string pollUrl)
        {
            if (string.IsNullOrWhiteSpace(pollUrl))
            {
                return "/";
            }

            if (Uri.TryCreate(pollUrl, UriKind.Absolute, out _))
            {
                return pollUrl;
            }

            return NormalizeEndpoint(pollUrl);
        }
    }
}
