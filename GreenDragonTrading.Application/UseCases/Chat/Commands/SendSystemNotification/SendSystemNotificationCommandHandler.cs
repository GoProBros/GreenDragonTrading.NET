using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Chat.Commands.SendSystemNotification
{
    public class SendSystemNotificationCommandHandler
        : IRequestHandler<SendSystemNotificationCommand, ApiResponse<SendSystemNotificationResponseDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<SendSystemNotificationCommandHandler> _logger;

        public SendSystemNotificationCommandHandler(
            IUnitOfWork uow,
            ILogger<SendSystemNotificationCommandHandler> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<ApiResponse<SendSystemNotificationResponseDto>> Handle(
            SendSystemNotificationCommand request,
            CancellationToken cancellationToken)
        {
            var message = request.Message.Trim();

            if (request.SendToAll)
            {
                var users = await _uow.Users.GetActiveUsersAsync(cancellationToken);
                if (users.Count == 0)
                {
                    throw new NotFoundException("Không tìm thấy người dùng hoạt động.");
                }

                var sentCount = 0;
                foreach (var user in users)
                {
                    await SendToSingleUserAsync(user.Id, message, cancellationToken);
                    sentCount++;
                }

                return ApiResponse<SendSystemNotificationResponseDto>.Success(
                    new SendSystemNotificationResponseDto
                    {
                        SentToAll = true,
                        SentCount = sentCount,
                        UserIds = users.Select(x => x.Id).ToList(),
                        Message = message,
                        MessageType = ChatMessageType.SystemNotification,
                        SessionType = ChatSessionType.System,
                        CreatedAt = DateTimeOffset.UtcNow,
                    },
                    $"Gửi thông báo hệ thống thành công cho {sentCount} người dùng.");
            }

            var userIds = (request.UserIds ?? [])
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (userIds.Count == 0)
            {
                throw new BusinessRuleException("Danh sách UserIds không hợp lệ.");
            }

            foreach (var userId in userIds)
            {
                await SendToSingleUserAsync(userId, message, cancellationToken);
            }

            return ApiResponse<SendSystemNotificationResponseDto>.Success(
                new SendSystemNotificationResponseDto
                {
                    SentToAll = false,
                    SentCount = userIds.Count,
                    UserIds = userIds,
                    Message = message,
                    MessageType = ChatMessageType.SystemNotification,
                    SessionType = ChatSessionType.System,
                    CreatedAt = DateTimeOffset.UtcNow,
                },
                $"Gửi thông báo hệ thống thành công cho {userIds.Count} người dùng.");
        }

        private async Task<SendSystemNotificationResponseDto> SendToSingleUserAsync(
            Guid userId,
            string message,
            CancellationToken cancellationToken)
        {
            var user = await _uow.Users.GetByIdAsync(userId, cancellationToken)
                ?? throw new NotFoundException("Không tìm thấy người dùng.");

            if (user.Status != CommonStatus.Active)
            {
                throw new BusinessRuleException("Người dùng hiện không hoạt động.");
            }

            var now = DateTimeOffset.UtcNow;
            var systemSession = await _uow.ChatSessions.GetSystemSessionByUserIdAsync(userId, cancellationToken);

            if (systemSession == null)
            {
                systemSession = new ChatSession
                {
                    Title = "System Notification",
                    SessionType = ChatSessionType.System,
                    Status = CommonStatus.Active,
                    CreatedBy = null,
                    CreatedAt = now,
                    UpdatedAt = now,
                };

                await _uow.ChatSessions.AddAsync(systemSession, cancellationToken);
                await _uow.SaveChangesAsync(cancellationToken);

                await _uow.ChatParticipants.AddAsync(new ChatParticipant
                {
                    SessionId = systemSession.Id,
                    UserId = userId,
                    Role = ChatRole.Member,
                    JoinedAt = now,
                    LastReadAt = now,
                }, cancellationToken);
            }
            else
            {
                var isParticipant = await _uow.ChatParticipants.IsUserParticipantAsync(systemSession.Id, userId, cancellationToken);
                if (!isParticipant)
                {
                    await _uow.ChatParticipants.AddAsync(new ChatParticipant
                    {
                        SessionId = systemSession.Id,
                        UserId = userId,
                        Role = ChatRole.Member,
                        JoinedAt = now,
                        LastReadAt = now,
                    }, cancellationToken);
                }
            }

            var systemMessage = new ChatMessage
            {
                SessionId = systemSession.Id,
                SenderId = null,
                Content = message,
                MessageType = ChatMessageType.SystemNotification,
                CreatedAt = now,
                UpdatedAt = now,
            };

            await _uow.ChatMessages.AddAsync(systemMessage, cancellationToken);

            systemSession.UpdatedAt = now;
            _uow.ChatSessions.Update(systemSession);

            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Sent system notification message {MessageId} to user {UserId} in session {SessionId}",
                systemMessage.Id,
                userId,
                systemSession.Id);

            return new SendSystemNotificationResponseDto
            {
                SessionId = systemSession.Id,
                MessageId = systemMessage.Id,
                UserId = userId,
                SentToAll = false,
                SentCount = 1,
                Message = systemMessage.Content,
                MessageType = ChatMessageType.SystemNotification,
                SessionType = ChatSessionType.System,
                CreatedAt = systemMessage.CreatedAt,
            };
        }
    }
}
