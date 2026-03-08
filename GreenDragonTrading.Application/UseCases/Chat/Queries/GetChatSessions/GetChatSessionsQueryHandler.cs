using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Chat.Queries.GetChatSessions
{
    public class GetChatSessionsQueryHandler : IRequestHandler<GetChatSessionsQuery, ApiResponse<List<ChatSessionListItemDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetChatSessionsQueryHandler> _logger;

        public GetChatSessionsQueryHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUserService,
            ILogger<GetChatSessionsQueryHandler> logger)
        {
            _uow = uow;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ApiResponse<List<ChatSessionListItemDto>>> Handle(GetChatSessionsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                throw new UnauthenticatedException("Bạn cần đăng nhập để xem danh sách chat.");
            }

            var sessions = await _uow.ChatSessions.GetSessionsWithParticipantsAsync(userId.Value, cancellationToken);

            var sessionDtos = sessions.Select(session =>
            {
                var participantCount = session.SessionType == ChatSessionType.AI || session.SessionType == ChatSessionType.System
                    ? 1
                    : session.Participants.Count;

                var creator = session.Participants.FirstOrDefault(p => p.UserId == session.CreatedBy);
                var creatorName = creator?.User?.Username;

                return new ChatSessionListItemDto
                {
                    Id = session.Id,
                    Title = session.Title,
                    SessionType = session.SessionType,
                    ParticipantCount = participantCount,
                    CreatorName = creatorName,
                    CreatedBy = session.CreatedBy,
                    CreatedAt = session.CreatedAt,
                    UpdatedAt = session.UpdatedAt
                };
            }).ToList();

            _logger.LogDebug("Retrieved {Count} chat sessions for user {UserId}", sessionDtos.Count, userId);

            return ApiResponse<List<ChatSessionListItemDto>>.Success(sessionDtos, "Lấy danh sách chat thành công.");
        }
    }
}
