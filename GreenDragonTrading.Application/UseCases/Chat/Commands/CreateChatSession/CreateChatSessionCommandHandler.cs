using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Chat.Commands.CreateChatSession
{ 
    public class CreateChatSessionCommandHandler : IRequestHandler<CreateChatSessionCommand, ApiResponse<ChatSessionDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CreateChatSessionCommandHandler> _logger;

        public CreateChatSessionCommandHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUserService,
            ILogger<CreateChatSessionCommandHandler> logger)
        {
            _uow = uow;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ApiResponse<ChatSessionDto>> Handle(CreateChatSessionCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                throw new UnauthenticatedException("Bạn cần đăng nhập để tạo phiên chat.");
            }

            var title = string.IsNullOrWhiteSpace(request.Title)
                ? $"Cuộc hội thoại mới - {DateTimeOffset.UtcNow:dd/MM/yyyy HH:mm}"
                : request.Title;
            
            var chatSession = new ChatSession
            {
                Title = title,
                SessionType = ChatSessionType.AI,
                Status = CommonStatus.Active,
                CreatedBy = userId.Value,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _uow.ChatSessions.AddAsync(chatSession, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            var participant = new ChatParticipant
            {
                SessionId = chatSession.Id,
                UserId = userId.Value,
                Role = ChatRole.Admin,
                JoinedAt = DateTimeOffset.UtcNow,
                LastReadAt = DateTimeOffset.UtcNow
            };

            await _uow.ChatParticipants.AddAsync(participant, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            var chatSessionDto = new ChatSessionDto
            {
                Id = chatSession.Id,
                Title = chatSession.Title,
                SessionType = chatSession.SessionType,
                Status = chatSession.Status,
                CreatedBy = chatSession.CreatedBy,
                CreatedAt = chatSession.CreatedAt,
                UpdatedAt = chatSession.UpdatedAt,
                LastReadAt = participant.LastReadAt,
                LastReadMessageId = participant.LastReadMessageId,
                Messages = new List<ChatMessageDto>()
            };

            _logger.LogInformation("Created new AI chat session: {SessionId} for user: {UserId}", chatSession.Id, userId);
            return ApiResponse<ChatSessionDto>.Success(chatSessionDto, "Tạo phiên chat thành công.");
        }
    }
}
