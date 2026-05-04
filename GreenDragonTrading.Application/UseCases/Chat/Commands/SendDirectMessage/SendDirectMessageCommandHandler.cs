using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs.Realtime;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Chat.Commands.SendDirectMessage
{
    public class SendDirectMessageCommandHandler
        : IRequestHandler<SendDirectMessageCommand, ApiResponse<DirectMessageDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationBroadcaster _notificationBroadcaster;
        private readonly ILogger<SendDirectMessageCommandHandler> _logger;

        public SendDirectMessageCommandHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUserService,
            INotificationBroadcaster notificationBroadcaster,
            ILogger<SendDirectMessageCommandHandler> logger)
        {
            _uow = uow;
            _currentUserService = currentUserService;
            _notificationBroadcaster = notificationBroadcaster;
            _logger = logger;
        }

        public async Task<ApiResponse<DirectMessageDto>> Handle(
            SendDirectMessageCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthenticatedException("Bạn cần đăng nhập để gửi tin nhắn.");

            var session = await _uow.ChatSessions.GetSessionWithMessagesAsync(request.SessionId, cancellationToken)
                ?? throw new NotFoundException("Không tìm thấy phiên trò chuyện.");

            if (session.SessionType != ChatSessionType.Direct)
                throw new BusinessRuleException("Endpoint này chỉ dành cho phiên trò chuyện trực tiếp.");

            if (session.Status != CommonStatus.Active)
                throw new BusinessRuleException("Phiên trò chuyện đã bị đóng.");

            var participant = session.Participants.FirstOrDefault(p => p.UserId == userId)
                ?? throw new AccessDeniedException("Bạn không phải là thành viên của cuộc trò chuyện này.");

            var now = DateTimeOffset.UtcNow;

            var message = new ChatMessage
            {
                SessionId = session.Id,
                SenderId = userId,
                Content = request.Content,
                MessageType = ChatMessageType.Text,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            };
            await _uow.ChatMessages.AddAsync(message, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            participant.LastReadAt = now;
            participant.LastReadMessageId = message.Id;
            _uow.ChatParticipants.Update(participant);

            session.UpdatedAt = now;
            _uow.ChatSessions.Update(session);

            await _uow.SaveChangesAsync(cancellationToken);

            var senderParticipant = session.Participants.FirstOrDefault(p => p.UserId == userId);
            var senderName = senderParticipant?.User?.Username ?? userId.ToString();

            var recipientIds = session.Participants
                .Where(p => p.UserId != userId)
                .Select(p => p.UserId)
                .Distinct()
                .ToList();

            foreach (var recipientId in recipientIds)
            {
                await _notificationBroadcaster.BroadcastDirectMessageAsync(
                    recipientId,
                    new DirectMessageSignalREventDto
                    {
                        SessionId = message.SessionId,
                        MessageId = message.Id,
                        SenderId = userId,
                        SenderName = senderName,
                        Content = message.Content,
                        CreatedAt = message.CreatedAt,
                    },
                    cancellationToken);
            }

            _logger.LogDebug("User {UserId} sent Direct message {MessageId} in session {SessionId}",
                userId, message.Id, session.Id);

            return ApiResponse<DirectMessageDto>.Success(new DirectMessageDto
            {
                Id = message.Id,
                SessionId = message.SessionId,
                SenderId = userId,
                SenderName = senderName,
                Content = message.Content,
                CreatedAt = message.CreatedAt,
                IsFromCurrentUser = true
            }, "Gửi tin nhắn thành công.");
        }
    }
}
