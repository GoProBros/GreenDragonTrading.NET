using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Chat.Queries.GetDirectChatSessions
{
    public class GetDirectChatSessionsQueryHandler
        : IRequestHandler<GetDirectChatSessionsQuery, ApiResponse<List<DirectSessionListItemDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetDirectChatSessionsQueryHandler> _logger;

        public GetDirectChatSessionsQueryHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUserService,
            ILogger<GetDirectChatSessionsQueryHandler> logger)
        {
            _uow = uow;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ApiResponse<List<DirectSessionListItemDto>>> Handle(
            GetDirectChatSessionsQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthenticatedException("Bạn cần đăng nhập để xem danh sách trò chuyện.");

            var sessions = await _uow.ChatSessions.GetDirectSessionsByUserIdAsync(userId, cancellationToken);

            var result = sessions.Select(session =>
            {
                var myParticipant = session.Participants.FirstOrDefault(p => p.UserId == userId);
                var otherParticipant = session.Participants.FirstOrDefault(p => p.UserId != userId);

                var lastMessage = session.Messages
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefault();

                var myLastReadAt = myParticipant?.LastReadAt;
                var hasUnread = lastMessage != null
                    && lastMessage.SenderId != userId
                    && (!myLastReadAt.HasValue || lastMessage.CreatedAt > myLastReadAt.Value);

                return new DirectSessionListItemDto
                {
                    SessionId = session.Id,
                    OtherParticipant = otherParticipant != null
                        ? new DirectChatParticipantDto
                        {
                            UserId = otherParticipant.UserId,
                            Username = otherParticipant.User?.Username ?? otherParticipant.UserId.ToString(),
                            AvatarUrl = otherParticipant.User?.AvatarUrl
                        }
                        : new DirectChatParticipantDto { UserId = Guid.Empty, Username = "Unknown" },
                    LastMessageContent = lastMessage?.Content,
                    LastMessageSenderId = lastMessage?.SenderId,
                    LastMessageAt = lastMessage?.CreatedAt,
                    MyLastReadAt = myLastReadAt,
                    MyLastReadMessageId = myParticipant?.LastReadMessageId,
                    HasUnread = hasUnread,
                    UpdatedAt = session.UpdatedAt
                };
            }).ToList();

            _logger.LogDebug("Retrieved {Count} Direct chat sessions for user {UserId}", result.Count, userId);

            return ApiResponse<List<DirectSessionListItemDto>>.Success(result, "Lấy danh sách trò chuyện thành công.");
        }
    }
}
