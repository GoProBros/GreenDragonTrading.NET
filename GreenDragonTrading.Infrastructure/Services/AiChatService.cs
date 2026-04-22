using GreenDragonTrading.Application.Common.Options;
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
        public async Task<AiChatResponse> SendMessageAsync(
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

            _logger.LogDebug("Sending message to AI Engine for conversation {ConversationId}", conversationId);

            HttpResponseMessage response;
            try
            {
                using var requestMessage = CreatePostRequest(NormalizeEndpoint(_options.ChatEndpoint), request);
                response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "AI Engine is unavailable for conversation {ConversationId}", conversationId);
                return new AiChatResponse
                {
                    Success = false,
                    ConversationId = conversationId,
                    Error = "AI Engine is currently unavailable. Please try again later."
                };
            }

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var syncBody = await response.Content.ReadFromJsonAsync<AiChatResponse>(cancellationToken: cancellationToken);
                if (syncBody == null)
                {
                    _logger.LogWarning("AI Engine returned null sync response for conversation {ConversationId}", conversationId);
                    return new AiChatResponse { Success = false, ConversationId = conversationId, Error = "AI Engine returned empty sync response" };
                }
                return syncBody;
            }

            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "AI Engine returned {StatusCode} for conversation {ConversationId}: {ErrorBody}",
                    response.StatusCode, conversationId, errorBody);

                return new AiChatResponse
                {
                    Success = false,
                    ConversationId = conversationId,
                    Error = $"AI Engine returned {(int)response.StatusCode}: {errorBody}"
                };
            }

            var accepted = await response.Content.ReadFromJsonAsync<AiChatAcceptedResponse>(cancellationToken: cancellationToken);
            if (accepted == null)
            {
                _logger.LogWarning("AI Engine returned null accepted response for conversation {ConversationId}", conversationId);
                return new AiChatResponse { Success = false, ConversationId = conversationId, Error = "AI Engine returned invalid accepted response" };
            }

            // Polling loop
            var pollPath = string.IsNullOrWhiteSpace(accepted.PollUrl)
                ? $"/api/chat/jobs/{accepted.JobId}"
                : accepted.PollUrl;

            // Make it an absolute path to the base address
            if (pollPath.StartsWith("/"))
            {
                pollPath = NormalizeEndpoint(pollPath);
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                HttpResponseMessage pollRes;
                try
                {
                    using var pollReq = new HttpRequestMessage(HttpMethod.Get, pollPath);
                    AttachAuthorizationHeader(pollReq);
                    pollRes = await _httpClient.SendAsync(pollReq, cancellationToken);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning(ex, "Polling failed for conversation {ConversationId}", conversationId);
                    return new AiChatResponse { Success = false, ConversationId = conversationId, Error = "Polling failed: " + ex.Message };
                }

                if (pollRes.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    continue;
                }

                if (pollRes.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Async job {JobId} not found or expired for {ConversationId}", accepted.JobId, conversationId);
                    return new AiChatResponse { Success = false, ConversationId = conversationId, Error = "Async job not found or expired." };
                }

                if (pollRes.IsSuccessStatusCode)
                {
                    var job = await pollRes.Content.ReadFromJsonAsync<AiChatJobStatusResponse>(cancellationToken: cancellationToken);
                    if (job == null)
                    {
                        return new AiChatResponse { Success = false, ConversationId = conversationId, Error = "Invalid poll response" };
                    }

                    if (string.Equals(job.Status, "completed", StringComparison.OrdinalIgnoreCase))
                    {
                        if (job.Result != null)
                        {
                            job.Result.ConversationId ??= conversationId;
                            return job.Result;
                        }
                        return new AiChatResponse { Success = false, ConversationId = conversationId, Error = "Empty completed result" };
                    }

                    if (string.Equals(job.Status, "failed", StringComparison.OrdinalIgnoreCase))
                    {
                        return new AiChatResponse { Success = false, ConversationId = conversationId, Error = $"Async chat failed: {job.Error}" };
                    }
                }
                else
                {
                    var err = await pollRes.Content.ReadAsStringAsync(cancellationToken);
                    return new AiChatResponse { Success = false, ConversationId = conversationId, Error = $"Unexpected poll status {pollRes.StatusCode}: {err}" };
                }
            }

            return new AiChatResponse { Success = false, ConversationId = conversationId, Error = "Polling cancelled" };
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
    }
}
