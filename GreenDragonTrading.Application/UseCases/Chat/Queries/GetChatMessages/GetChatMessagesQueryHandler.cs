using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Chat.Queries.GetChatMessages
{
    public class GetChatMessagesQueryHandler : IRequestHandler<GetChatMessagesQuery, ApiResponse<ChatSessionDetailDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetChatMessagesQueryHandler> _logger;

        public GetChatMessagesQueryHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUserService,
            ILogger<GetChatMessagesQueryHandler> logger)
        {
            _uow = uow;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ApiResponse<ChatSessionDetailDto>> Handle(GetChatMessagesQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                throw new UnauthenticatedException("Bạn cần đăng nhập để xem nội dung chat.");
            }

            var session = await _uow.ChatSessions.GetSessionWithMessagesAsync(request.SessionId, cancellationToken);

            if (session == null)
            {
                throw new NotFoundException("Không tìm thấy phiên chat.");
            }

            if (session.SessionType == ChatSessionType.AI && session.CreatedBy != userId)
            {
                throw new AccessDeniedException("Bạn không có quyền xem phiên chat này.");
            }

            var chatSessionDto = new ChatSessionDetailDto
            {
                Id = session.Id,
                Title = session.Title,
                Summary = session.ConversationSummary,
                Messages = session.Messages
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new ChatMessageSimpleDto
                    {
                        Role = m.SenderId == null ? "ai" : "user",
                        Content = m.Content
                    })
                    .ToList()
            };

            _logger.LogDebug("Retrieved chat session {SessionId} with {MessageCount} messages for user {UserId}", 
                session.Id, chatSessionDto.Messages.Count, userId);

            return ApiResponse<ChatSessionDetailDto>.Success(chatSessionDto, "Lấy nội dung chat thành công.");
        }
    }
}
