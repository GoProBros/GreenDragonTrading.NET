using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.DTOs.Realtime;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.Chat.Commands.SendChatMessage
{
    public class SendChatMessageCommandHandler : IRequestHandler<SendChatMessageCommand, ApiResponse<SendChatMessageResultDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAiChatService _aiChatService;
        private readonly IChatAsyncJobService _chatAsyncJobService;
        private readonly INotificationBroadcaster _notificationBroadcaster;
        private readonly ILogger<SendChatMessageCommandHandler> _logger;
        private readonly AiEngineOptions _aiOptions;

        public SendChatMessageCommandHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUserService,
            IAiChatService aiChatService,
            IChatAsyncJobService chatAsyncJobService,
            INotificationBroadcaster notificationBroadcaster,
            IOptions<AiEngineOptions> aiOptions,
            ILogger<SendChatMessageCommandHandler> logger)
        {
            _uow = uow;
            _currentUserService = currentUserService;
            _aiChatService = aiChatService;
            _chatAsyncJobService = chatAsyncJobService;
            _notificationBroadcaster = notificationBroadcaster;
            _aiOptions = aiOptions.Value;
            _logger = logger;
        }

        public async Task<ApiResponse<SendChatMessageResultDto>> Handle(
            SendChatMessageCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                throw new UnauthenticatedException("Bạn cần đăng nhập để gửi tin nhắn.");
            }

            var session = await _uow.ChatSessions.GetByIdAsync(request.SessionId, cancellationToken);
            if (session == null)
            {
                throw new NotFoundException("Không tìm thấy phiên chat.");
            }

            if (session.Status != CommonStatus.Active)
            {
                throw new BusinessRuleException("Phiên chat đã bị đóng.");
            }

            if (session.SessionType == ChatSessionType.System)
            {
                throw new BusinessRuleException("Không thể gửi tin nhắn người dùng vào phiên System.");
            }

            var isParticipant = await _uow.ChatParticipants.IsUserParticipantAsync(
                request.SessionId, userId.Value, cancellationToken);
            if (!isParticipant)
            {
                throw new AccessDeniedException("Bạn không phải thành viên của phiên chat này.");
            }

            var userMessage = new ChatMessage
            {
                SessionId = request.SessionId,
                SenderId = userId.Value,
                Content = request.Message,
                MessageType = ChatMessageType.Text,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _uow.ChatMessages.AddAsync(userMessage, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Saved user message {MessageId} in session {SessionId} from user {UserId}",
                userMessage.Id, request.SessionId, userId);

            var recentMessages = await _uow.ChatMessages.GetRecentMessagesAsync(
                request.SessionId, _aiOptions.RecentMessagesLimit, cancellationToken);

            var context = new AiChatContext
            {
                Summary = session.ConversationSummary,
                RecentMessages = recentMessages.Select(m => new AiMessageInput
                {
                    Role = m.SenderId == null ? "assistant" : "user",
                    Content = m.Content
                }).ToList()
            };

            var conversationId = $"session-{request.SessionId}";
            var submitResult = await _aiChatService.SubmitMessageAsync(
                conversationId, request.Message, context, cancellationToken);

            if (submitResult.Accepted && submitResult.AcceptedResponse != null)
            {
                var accepted = submitResult.AcceptedResponse;

                if (string.IsNullOrWhiteSpace(accepted.JobId))
                {
                    var fallbackResult = await PersistAiResponseAsync(
                        request.SessionId,
                        userId.Value,
                        userMessage,
                        new AiChatResponse
                        {
                            Success = false,
                            ConversationId = conversationId,
                            Error = "AI Engine accepted request without a valid job identifier."
                        },
                        cancellationToken);

                    return ApiResponse<SendChatMessageResultDto>.Success(
                        new SendChatMessageResultDto
                        {
                            Accepted = false,
                            Status = ChatAsyncJobStatuses.Completed,
                            UserMessage = fallbackResult.UserMessage,
                            Result = fallbackResult
                        },
                        "Gửi tin nhắn thành công.");
                }

                var aiPollUrl = string.IsNullOrWhiteSpace(accepted.PollUrl)
                    ? $"/api/chat/jobs/{accepted.JobId}"
                    : accepted.PollUrl;

                await _chatAsyncJobService.EnqueueAsync(
                    new ChatAsyncProcessingJob
                    {
                        JobId = accepted.JobId,
                        PollUrl = aiPollUrl,
                        ConversationId = conversationId,
                        SessionId = request.SessionId,
                        UserId = userId.Value,
                        UserMessageId = userMessage.Id,
                        CreatedAt = DateTimeOffset.UtcNow
                    },
                    cancellationToken);

                var acceptedResult = new SendChatMessageResultDto
                {
                    Accepted = true,
                    Status = string.IsNullOrWhiteSpace(accepted.Status)
                        ? ChatAsyncJobStatuses.Queued
                        : accepted.Status,
                    JobId = accepted.JobId,
                    PollUrl = $"/api/v1/chat/jobs/{accepted.JobId}",
                    IntentHint = accepted.IntentHint,
                    UserMessage = MapToDto(userMessage)
                };

                return ApiResponse<SendChatMessageResultDto>.Success(
                    acceptedResult,
                    "Tin nhắn đã được tiếp nhận và đang xử lý.");
            }

            var aiResponse = submitResult.Response ?? new AiChatResponse
            {
                Success = false,
                ConversationId = conversationId,
                Error = submitResult.Error ?? "AI Engine returned empty response"
            };

            var completedResult = await PersistAiResponseAsync(
                request.SessionId,
                userId.Value,
                userMessage,
                aiResponse,
                cancellationToken);

            return ApiResponse<SendChatMessageResultDto>.Success(
                new SendChatMessageResultDto
                {
                    Accepted = false,
                    Status = ChatAsyncJobStatuses.Completed,
                    UserMessage = completedResult.UserMessage,
                    Result = completedResult
                },
                "Gửi tin nhắn thành công.");
        }

        private async Task<SendChatMessageResponseDto> PersistAiResponseAsync(
            int sessionId,
            Guid userId,
            ChatMessage userMessage,
            AiChatResponse aiResponse,
            CancellationToken cancellationToken)
        {
            var aiContent = !string.IsNullOrWhiteSpace(aiResponse.Response)
                ? aiResponse.Response
                : "Toi chua co du thong tin de tra loi cau hoi nay.";

            var aiMessage = new ChatMessage
            {
                SessionId = sessionId,
                SenderId = null,
                Content = aiContent,
                ResponseData = aiResponse.ResponseData != null
                    ? JsonSerializer.Serialize(aiResponse.ResponseData)
                    : null,
                MessageType = ChatMessageType.Text,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _uow.ChatMessages.AddAsync(aiMessage, cancellationToken);

            var session = await _uow.ChatSessions.GetByIdAsync(sessionId, cancellationToken)
                ?? throw new NotFoundException("Không tìm thấy phiên chat.");

            session.UpdatedAt = DateTimeOffset.UtcNow;
            _uow.ChatSessions.Update(session);

            await _uow.SaveChangesAsync(cancellationToken);

            await _notificationBroadcaster.BroadcastAiChatResponseAsync(
                userId,
                new AiChatResponseSignalREventDto
                {
                    SessionId = sessionId,
                    UserMessageId = userMessage.Id,
                    AiMessageId = aiMessage.Id,
                    Content = aiMessage.Content,
                    CreatedAt = aiMessage.CreatedAt,
                },
                cancellationToken);

            _logger.LogInformation(
                "Saved AI response {MessageId} in session {SessionId}",
                aiMessage.Id,
                sessionId);

            await TryUpdateSummaryAsync(sessionId, cancellationToken);

            return new SendChatMessageResponseDto
            {
                UserMessage = MapToDto(userMessage),
                AiMessage = MapToDto(aiMessage),
                Intents = aiResponse.Intents?.Select(i => new AiIntentDto
                {
                    Intent = i.Intent,
                    Confidence = i.Confidence
                }).ToList()
            };
        }

        private async Task TryUpdateSummaryAsync(int sessionId, CancellationToken cancellationToken)
        {
            try
            {
                var session = await _uow.ChatSessions.GetByIdAsync(sessionId, cancellationToken);
                if (session == null)
                {
                    return;
                }

                List<ChatMessage> messagesToSummarize;
                if (string.IsNullOrWhiteSpace(session.ConversationSummary) || session.LastSummaryMessageId == null)
                {
                    messagesToSummarize = await _uow.ChatMessages.GetAllMessagesBySessionIdAsync(
                        sessionId,
                        cancellationToken);
                }
                else
                {
                    messagesToSummarize = await _uow.ChatMessages.GetMessagesAfterIdAsync(
                        sessionId,
                        session.LastSummaryMessageId.Value,
                        cancellationToken);
                }

                if (messagesToSummarize.Count < _aiOptions.RecentMessagesLimit)
                {
                    _logger.LogDebug(
                        "Skip summary update for session {SessionId}: pending message count {PendingCount} is below threshold {Threshold}",
                        sessionId,
                        messagesToSummarize.Count,
                        _aiOptions.RecentMessagesLimit);
                    return;
                }

                var messagesForSummary = messagesToSummarize
                    .Select(m => new AiMessageInput
                    {
                        Role = m.SenderId == null ? "assistant" : "user",
                        Content = m.Content
                    })
                    .ToList();

                var conversationId = $"session-{sessionId}";

                var result = await _aiChatService.UpdateSummaryAsync(
                    conversationId,
                    session.ConversationSummary,
                    messagesForSummary,
                    cancellationToken);

                if (result.Success && !string.IsNullOrEmpty(result.UpdatedSummary))
                {
                    session.ConversationSummary = result.UpdatedSummary;
                    session.LastSummaryMessageId = messagesToSummarize[^1].Id;
                    session.UpdatedAt = DateTimeOffset.UtcNow;
                    _uow.ChatSessions.Update(session);
                    await _uow.SaveChangesAsync(cancellationToken);
                    _logger.LogDebug(
                        "Updated summary for session {SessionId}, lastMessageId={MessageId}",
                        sessionId,
                        messagesToSummarize[^1].Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update conversation summary for session {SessionId}", sessionId);
            }
        }

        private static ChatMessageDto MapToDto(ChatMessage message)
        {
            return new ChatMessageDto
            {
                Id = message.Id,
                SessionId = message.SessionId,
                SenderId = message.SenderId,
                Content = message.Content,
                ResponseData = message.ResponseData,
                MessageType = message.MessageType,
                FileUrl = message.FileUrl,
                FileName = message.FileName,
                FileSize = message.FileSize,
                CreatedAt = message.CreatedAt,
                UpdatedAt = message.UpdatedAt
            };
        }
    }
}
