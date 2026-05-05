using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
namespace GreenDragonTrading.Application.UseCases.Chat.Commands.MarkAllSessionsAsRead;

public class MarkAllSessionsAsReadCommandHandler : IRequestHandler<MarkAllSessionsAsReadCommand, ApiResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<MarkAllSessionsAsReadCommandHandler> _logger;

    public MarkAllSessionsAsReadCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        ILogger<MarkAllSessionsAsReadCommandHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse> Handle(MarkAllSessionsAsReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            throw new UnauthenticatedException("Bạn cần đăng nhập để đánh dấu đã đọc.");
        }

        // Get all participant records belonging to this user
        var participants = (await _uow.ChatParticipants.FindAsync(
            p => p.UserId == userId.Value,
            cancellationToken)).ToList();

        if (participants.Count == 0)
        {
            return ApiResponse.Success("Không có phiên chat nào cần đánh dấu.");
        }

        var now = DateTimeOffset.UtcNow;

        foreach (var participant in participants)
        {
            participant.LastReadAt = now;

            // Also update LastReadMessageId to the most recent message in each session
            var recentMessages = await _uow.ChatMessages.GetRecentMessagesAsync(
                participant.SessionId,
                1,
                cancellationToken);

            if (recentMessages.Count > 0)
            {
                participant.LastReadMessageId = recentMessages[0].Id;
            }

            _uow.ChatParticipants.Update(participant);
        }

        await _uow.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Marked all {Count} session(s) as read for user {UserId} at {ReadAt}",
            participants.Count,
            userId,
            now);

        return ApiResponse.Success("Đánh dấu tất cả đã đọc thành công.");
    }
}
