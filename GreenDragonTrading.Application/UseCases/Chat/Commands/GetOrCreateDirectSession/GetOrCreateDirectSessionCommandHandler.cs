using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Chat.Commands.GetOrCreateDirectSession
{
    public class GetOrCreateDirectSessionCommandHandler
        : IRequestHandler<GetOrCreateDirectSessionCommand, ApiResponse<DirectChatSessionDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetOrCreateDirectSessionCommandHandler> _logger;

        public GetOrCreateDirectSessionCommandHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUserService,
            ILogger<GetOrCreateDirectSessionCommandHandler> logger)
        {
            _uow = uow;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ApiResponse<DirectChatSessionDto>> Handle(
            GetOrCreateDirectSessionCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthenticatedException("Bạn cần đăng nhập để sử dụng tính năng này.");

            var targetUser = await _uow.Users.FindByPhoneOrEmailAsync(request.PhoneOrEmail, cancellationToken)
                ?? throw new NotFoundException("Không tìm thấy người dùng với thông tin đã cung cấp.");

            if (targetUser.Id == userId)
                throw new BusinessRuleException("Không thể tạo cuộc trò chuyện với chính mình.");

            if (targetUser.Status != CommonStatus.Active)
                throw new BusinessRuleException("Người dùng này hiện không hoạt động.");

            var existing = await _uow.ChatSessions.GetDirectSessionBetweenUsersAsync(userId, targetUser.Id, cancellationToken);
            if (existing != null)
            {
                var myParticipant = existing.Participants.FirstOrDefault(p => p.UserId == userId);
                var other = existing.Participants.FirstOrDefault(p => p.UserId == targetUser.Id);

                _logger.LogDebug("Returning existing Direct session {SessionId} for users {UserA} and {UserB}",
                    existing.Id, userId, targetUser.Id);

                return ApiResponse<DirectChatSessionDto>.Success(new DirectChatSessionDto
                {
                    SessionId = existing.Id,
                    IsNew = false,
                    OtherParticipant = new DirectChatParticipantDto
                    {
                        UserId = targetUser.Id,
                        Username = targetUser.Username,
                        AvatarUrl = targetUser.AvatarUrl
                    },
                    CreatedAt = existing.CreatedAt,
                    UpdatedAt = existing.UpdatedAt,
                    MyLastReadAt = myParticipant?.LastReadAt,
                    MyLastReadMessageId = myParticipant?.LastReadMessageId
                }, "Lấy cuộc trò chuyện thành công.");
            }

            var now = DateTimeOffset.UtcNow;
            var session = new ChatSession
            {
                Title = $"Direct: {targetUser.Username}",
                SessionType = ChatSessionType.Direct,
                Status = CommonStatus.Active,
                CreatedBy = null,
                CreatedAt = now,
                UpdatedAt = now
            };
            await _uow.ChatSessions.AddAsync(session, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            var participants = new List<ChatParticipant>
            {
                new ChatParticipant
                {
                    SessionId = session.Id,
                    UserId = userId,
                    Role = ChatRole.Member,
                    JoinedAt = now,
                    LastReadAt = now
                },
                new ChatParticipant
                {
                    SessionId = session.Id,
                    UserId = targetUser.Id,
                    Role = ChatRole.Member,
                    JoinedAt = now,
                    LastReadAt = now
                }
            };
            await _uow.ChatParticipants.AddRangeAsync(participants, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created new Direct session {SessionId} between {UserA} and {UserB}",
                session.Id, userId, targetUser.Id);

            return ApiResponse<DirectChatSessionDto>.Success(new DirectChatSessionDto
            {
                SessionId = session.Id,
                IsNew = true,
                OtherParticipant = new DirectChatParticipantDto
                {
                    UserId = targetUser.Id,
                    Username = targetUser.Username,
                    AvatarUrl = targetUser.AvatarUrl
                },
                CreatedAt = session.CreatedAt,
                UpdatedAt = session.UpdatedAt,
                MyLastReadAt = now,
                MyLastReadMessageId = null
            }, "Tạo cuộc trò chuyện mới thành công.");
        }
    }
}
