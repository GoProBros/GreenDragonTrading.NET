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
                Context = context
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

            if (!response.IsSuccessStatusCode)
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

            var aiResponse = await response.Content.ReadFromJsonAsync<AiChatResponse>(cancellationToken: cancellationToken);

            if (aiResponse == null)
            {
                _logger.LogWarning("AI Engine returned null response for conversation {ConversationId}", conversationId);
                return new AiChatResponse
                {
                    Success = false,
                    ConversationId = conversationId,
                    Error = "AI Engine returned empty response"
                };
            }

            _logger.LogDebug("Received AI response for conversation {ConversationId}, success: {Success}", conversationId, aiResponse.Success);
            return aiResponse;
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
