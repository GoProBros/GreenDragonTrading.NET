using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Chat.Commands.MarkSessionAsRead;

public class MarkSessionAsReadCommandHandler : IRequestHandler<MarkSessionAsReadCommand, ApiResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<MarkSessionAsReadCommandHandler> _logger;

    public MarkSessionAsReadCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        ILogger<MarkSessionAsReadCommandHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse> Handle(MarkSessionAsReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            throw new UnauthenticatedException("Bạn cần đăng nhập để đánh dấu đã đọc.");
        }

        var session = await _uow.ChatSessions.GetByIdAsync(request.SessionId, cancellationToken);
        if (session == null || session.Status != CommonStatus.Active)
        {
            throw new NotFoundException("Không tìm thấy phiên chat.");
        }

        var participant = await _uow.ChatParticipants.FirstOrDefaultAsync(
            x => x.SessionId == request.SessionId && x.UserId == userId.Value,
            cancellationToken);

        if (participant == null)
        {
            throw new AccessDeniedException("Bạn không phải thành viên của phiên chat này.");
        }

        var now = DateTimeOffset.UtcNow;
        participant.LastReadAt = now;

        var recentMessages = await _uow.ChatMessages.GetRecentMessagesAsync(
            request.SessionId,
            1,
            cancellationToken);

        if (recentMessages.Count > 0)
        {
            participant.LastReadMessageId = recentMessages[0].Id;
        }

        _uow.ChatParticipants.Update(participant);
        await _uow.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Marked session {SessionId} as read for user {UserId} at {ReadAt}",
            request.SessionId,
            userId,
            now);

        return ApiResponse.Success("Đánh dấu đã đọc thành công.");
    }
}
