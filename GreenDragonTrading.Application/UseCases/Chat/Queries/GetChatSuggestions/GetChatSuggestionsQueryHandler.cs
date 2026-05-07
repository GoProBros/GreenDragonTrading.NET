using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Chat.Queries.GetChatSuggestions
{
    /// <summary>
    /// Handles retrieval of AI-powered chat question suggestions by proxying to the AI Engine.
    /// </summary>
    public class GetChatSuggestionsQueryHandler : IRequestHandler<GetChatSuggestionsQuery, ApiResponse<ChatSuggestionsDto>>
    {
        private readonly IAiChatService _aiChatService;
        private readonly ILogger<GetChatSuggestionsQueryHandler> _logger;

        public GetChatSuggestionsQueryHandler(
            IAiChatService aiChatService,
            ILogger<GetChatSuggestionsQueryHandler> logger)
        {
            _aiChatService = aiChatService;
            _logger = logger;
        }

        public async Task<ApiResponse<ChatSuggestionsDto>> Handle(GetChatSuggestionsQuery request, CancellationToken cancellationToken)
        {
            var suggestions = await _aiChatService.GetSuggestionsAsync(cancellationToken);

            if (suggestions == null || !suggestions.Success)
            {
                _logger.LogWarning("Failed to get chat suggestions from AI Engine");
                return ApiResponse<ChatSuggestionsDto>.Failure("Không thể tạo gợi ý câu hỏi. Vui lòng thử lại sau.");
            }

            var dto = new ChatSuggestionsDto
            {
                Questions = suggestions.Questions ?? new List<string>(),
                SourceSymbols = suggestions.SourceSymbols ?? new List<string>(),
                Metadata = suggestions.Metadata != null
                    ? new ChatSuggestionsMetadataDto
                    {
                        ModelName = suggestions.Metadata.ModelName,
                        LatencyMs = suggestions.Metadata.LatencyMs
                    }
                    : null
            };

            _logger.LogDebug("Retrieved {Count} chat suggestions for user", dto.Questions.Count);

            return ApiResponse<ChatSuggestionsDto>.Success(dto, "Lấy gợi ý câu hỏi thành công.");
        }
    }
}
