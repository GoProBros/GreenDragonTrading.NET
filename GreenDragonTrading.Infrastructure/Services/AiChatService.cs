using GreenDragonTrading.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace GreenDragonTrading.Infrastructure.Services
{
    /// <summary>
    /// Service for communicating with the GreenDragonTrading AI Engine
    /// </summary>
    public class AiChatService : IAiChatService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AiChatService> _logger;

        public AiChatService(HttpClient httpClient, ILogger<AiChatService> logger)
        {
            _httpClient = httpClient;
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
                response = await _httpClient.PostAsJsonAsync("/api/chat", request, cancellationToken);
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
                response = await _httpClient.PostAsJsonAsync("/api/conv-summary", request, cancellationToken);
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
    }
}
