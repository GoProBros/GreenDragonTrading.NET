using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GreenDragonTrading.Application.UseCases.Chat.Commands.SummarizeSession;
public class SummarizeSessionCommandHandler : IRequestHandler<SummarizeSessionCommand, ApiResponse<SummarizeSessionResponseDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAiChatService _aiChatService;
    private readonly AiEngineOptions _aiOptions;
    private readonly ILogger<SummarizeSessionCommandHandler> _logger;

    public SummarizeSessionCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        IAiChatService aiChatService,
        IOptions<AiEngineOptions> aiOptions,
        ILogger<SummarizeSessionCommandHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _aiChatService = aiChatService;
        _aiOptions = aiOptions.Value;
        _logger = logger;
    }

    public async Task<ApiResponse<SummarizeSessionResponseDto>> Handle(
        SummarizeSessionCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            throw new UnauthenticatedException("Người dùng chưa đăng nhập");

        var session = await _uow.ChatSessions.GetByIdAsync(request.SessionId, cancellationToken)
            ?? throw new NotFoundException("Phiên trò chuyện không tồn tại");

        if (session.CreatedBy != userId.Value)
            throw new AccessDeniedException("Bạn không có quyền truy cập phiên trò chuyện này");

        var conversationId = $"session-{session.Id}";

        List<Domain.Entities.ChatMessage> messagesToSummarize;
        if (session.ConversationSummary == null || session.LastSummaryMessageId == null)
        {
            messagesToSummarize = await _uow.ChatMessages.GetAllMessagesBySessionIdAsync(
                session.Id, cancellationToken);
        }
        else
        {
            messagesToSummarize = await _uow.ChatMessages.GetMessagesAfterIdAsync(
                session.Id, session.LastSummaryMessageId.Value, cancellationToken);
        }

        if (messagesToSummarize.Count == 0)
        {
            _logger.LogInformation("No new messages to summarize for session {SessionId}", session.Id);
            return ApiResponse<SummarizeSessionResponseDto>.Success(new SummarizeSessionResponseDto
            {
                SessionId = session.Id,
                UpdatedSummary = session.ConversationSummary ?? string.Empty,
                LastSummaryMessageId = session.LastSummaryMessageId ?? 0,
                ProcessedMessages = 0
            }, "Không có tin nhắn mới cần tóm tắt");
        }

        if (messagesToSummarize.Count < _aiOptions.RecentMessagesLimit)
        {
            var recentMessages = await _uow.ChatMessages.GetRecentMessagesAsync(
                session.Id,
                _aiOptions.RecentMessagesLimit,
                cancellationToken);

            _logger.LogInformation(
                "Skip summarizing session {SessionId} because pending message count {PendingCount} is less than threshold {Threshold}",
                session.Id,
                messagesToSummarize.Count,
                _aiOptions.RecentMessagesLimit);

            return ApiResponse<SummarizeSessionResponseDto>.Success(new SummarizeSessionResponseDto
            {
                SessionId = session.Id,
                UpdatedSummary = session.ConversationSummary ?? string.Empty,
                LastSummaryMessageId = session.LastSummaryMessageId ?? 0,
                ProcessedMessages = recentMessages.Count
            }, "Số lượng tin nhắn chưa đủ ngưỡng để tóm tắt. Đã trả về số lượng tin gần nhất theo cấu hình.");
        }

        var messagesForSummary = messagesToSummarize
            .Skip(Math.Max(0, messagesToSummarize.Count - _aiOptions.RecentMessagesLimit))
            .ToList();

        var aiMessages = messagesForSummary
            .Select(m => new AiMessageInput
            {
                Role = m.SenderId == null ? "assistant" : "user",
                Content = m.Content
            })
            .ToList();

        var result = await _aiChatService.UpdateSummaryAsync(
            conversationId, session.ConversationSummary, aiMessages, cancellationToken);

        if (!result.Success || string.IsNullOrEmpty(result.UpdatedSummary))
        {
            _logger.LogWarning("AI summary update failed for session {SessionId}", session.Id);
            throw new BusinessRuleException("Không thể tạo tóm tắt cuộc trò chuyện. Vui lòng thử lại sau.");
        }

        session.ConversationSummary = result.UpdatedSummary;
        session.LastSummaryMessageId = messagesToSummarize[^1].Id;
        session.UpdatedAt = DateTimeOffset.UtcNow;
        _uow.ChatSessions.Update(session);
        await _uow.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Summary updated for session {SessionId}, processed {Count} messages",
            session.Id, messagesToSummarize.Count);

        return ApiResponse<SummarizeSessionResponseDto>.Success(new SummarizeSessionResponseDto
        {
            SessionId = session.Id,
            UpdatedSummary = result.UpdatedSummary,
            LastSummaryMessageId = messagesToSummarize[^1].Id,
            ProcessedMessages = messagesToSummarize.Count
        }, "Tóm tắt cuộc trò chuyện thành công");
    }
}
