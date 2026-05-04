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

            var participant = session.Participants.FirstOrDefault(x => x.UserId == userId.Value);
            if (participant == null)
            {
                var isAiOwner = session.SessionType == ChatSessionType.AI && session.CreatedBy == userId;
                if (!isAiOwner)
                {
                    throw new AccessDeniedException("Bạn không có quyền xem phiên chat này.");
                }
            }

            var lastReadAt = participant?.LastReadAt;

            DirectChatParticipantDto? otherParticipant = null;
            if (session.SessionType == ChatSessionType.Direct)
            {
                var other = session.Participants.FirstOrDefault(p => p.UserId != userId.Value);
                if (other != null)
                {
                    otherParticipant = new DirectChatParticipantDto
                    {
                        UserId = other.UserId,
                        Username = other.User?.Username ?? other.UserId.ToString(),
                        AvatarUrl = other.User?.AvatarUrl
                    };
                }
            }

            var chatSessionDto = new ChatSessionDetailDto
            {
                Id = session.Id,
                Title = session.Title,
                SessionType = session.SessionType,
                Summary = session.ConversationSummary,
                LastReadAt = lastReadAt,
                LastReadMessageId = participant?.LastReadMessageId,
                OtherParticipant = otherParticipant,
                Messages = session.Messages
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new ChatMessageSimpleDto
                    {
                        Id = m.Id,
                        Role = m.SenderId == null ? "ai" : "user",
                        SenderId = m.SenderId,
                        SenderName = m.Sender?.Username,
                        Content = m.Content,
                        CreatedAt = m.CreatedAt,
                        IsUnreadForCurrentUser =
                            m.SenderId != userId
                            && (!lastReadAt.HasValue || m.CreatedAt > lastReadAt.Value)
                    })
                    .ToList()
            };

            _logger.LogDebug("Retrieved chat session {SessionId} with {MessageCount} messages for user {UserId}", 
                session.Id, chatSessionDto.Messages.Count, userId);

            return ApiResponse<ChatSessionDetailDto>.Success(chatSessionDto, "Lấy nội dung chat thành công.");
        }
    }
}
