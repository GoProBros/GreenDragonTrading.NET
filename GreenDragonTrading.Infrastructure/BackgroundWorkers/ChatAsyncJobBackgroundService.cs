using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.DTOs.Realtime;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace GreenDragonTrading.Infrastructure.BackgroundWorkers
{
    /// <summary>
    /// Processes queued AI chat jobs and persists final AI responses asynchronously.
    /// </summary>
    public class ChatAsyncJobBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        IChatAsyncJobService chatAsyncJobService,
        IOptions<AiEngineOptions> aiOptions,
        ILogger<ChatAsyncJobBackgroundService> logger) : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
        private readonly IChatAsyncJobService _chatAsyncJobService = chatAsyncJobService;
        private readonly AiEngineOptions _aiOptions = aiOptions.Value;
        private readonly ILogger<ChatAsyncJobBackgroundService> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Chat async job worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _chatAsyncJobService.CleanupExpiredAsync(stoppingToken);

                    var job = await _chatAsyncJobService.DequeueAsync(stoppingToken);
                    await _chatAsyncJobService.MarkRunningAsync(job.JobId, stoppingToken);

                    await ProcessJobAsync(job, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in chat async job worker loop.");
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
            }

            _logger.LogInformation("Chat async job worker stopped.");
        }

        private async Task ProcessJobAsync(ChatAsyncProcessingJob job, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Processing async chat job {JobId} for session {SessionId}",
                job.JobId,
                job.SessionId);

            using var scope = _serviceScopeFactory.CreateScope();
            var aiChatService = scope.ServiceProvider.GetRequiredService<IAiChatService>();

            var pollingInterval = _aiOptions.ChatAsyncPollingIntervalSeconds <= 0
                ? 2
                : _aiOptions.ChatAsyncPollingIntervalSeconds;

            while (!cancellationToken.IsCancellationRequested)
            {
                var aiJobStatus = await aiChatService.GetJobStatusAsync(job.PollUrl, cancellationToken);
                if (aiJobStatus == null)
                {
                    await _chatAsyncJobService.MarkFailedAsync(job.JobId, "AI Engine returned invalid poll response.", cancellationToken);
                    return;
                }

                if (string.Equals(aiJobStatus.Status, ChatAsyncJobStatuses.Queued, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(aiJobStatus.Status, ChatAsyncJobStatuses.Running, StringComparison.OrdinalIgnoreCase))
                {
                    await Task.Delay(TimeSpan.FromSeconds(pollingInterval), cancellationToken);
                    continue;
                }

                if (string.Equals(aiJobStatus.Status, ChatAsyncJobStatuses.Failed, StringComparison.OrdinalIgnoreCase))
                {
                    await _chatAsyncJobService.MarkFailedAsync(
                        job.JobId,
                        aiJobStatus.Error ?? "Async chat failed in AI Engine.",
                        cancellationToken);
                    return;
                }

                if (!string.Equals(aiJobStatus.Status, ChatAsyncJobStatuses.Completed, StringComparison.OrdinalIgnoreCase))
                {
                    await _chatAsyncJobService.MarkFailedAsync(
                        job.JobId,
                        $"Unexpected async job status: {aiJobStatus.Status}",
                        cancellationToken);
                    return;
                }

                if (aiJobStatus.Result == null)
                {
                    await _chatAsyncJobService.MarkFailedAsync(job.JobId, "AI Engine completed without result.", cancellationToken);
                    return;
                }

                try
                {
                    var persistedResult = await PersistAiResponseAsync(
                        scope.ServiceProvider,
                        job,
                        aiJobStatus.Result,
                        cancellationToken);

                    await _chatAsyncJobService.MarkCompletedAsync(job.JobId, persistedResult, cancellationToken);
                    _logger.LogInformation("Completed async chat job {JobId}", job.JobId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed processing completed async chat job {JobId}", job.JobId);
                    await _chatAsyncJobService.MarkFailedAsync(job.JobId, ex.Message, cancellationToken);
                }

                return;
            }
        }

        private async Task<SendChatMessageResponseDto> PersistAiResponseAsync(
            IServiceProvider serviceProvider,
            ChatAsyncProcessingJob job,
            AiChatResponse aiResponse,
            CancellationToken cancellationToken)
        {
            var uow = serviceProvider.GetRequiredService<IUnitOfWork>();
            var notificationBroadcaster = serviceProvider.GetRequiredService<INotificationBroadcaster>();
            var aiChatService = serviceProvider.GetRequiredService<IAiChatService>();

            var session = await uow.ChatSessions.GetByIdAsync(job.SessionId, cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy phiên chat để xử lý kết quả async.");

            var userMessage = await uow.ChatMessages.GetByIdAsync(job.UserMessageId, cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy user message cho async job.");

            var aiContent = !string.IsNullOrWhiteSpace(aiResponse.Response)
                ? aiResponse.Response
                : "Toi chua co du thong tin de tra loi cau hoi nay.";

            var aiMessage = new ChatMessage
            {
                SessionId = session.Id,
                SenderId = null,
                Content = aiContent,
                ResponseData = aiResponse.ResponseData != null
                    ? JsonSerializer.Serialize(aiResponse.ResponseData)
                    : null,
                MessageType = ChatMessageType.Text,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await uow.ChatMessages.AddAsync(aiMessage, cancellationToken);
            session.UpdatedAt = DateTimeOffset.UtcNow;
            uow.ChatSessions.Update(session);
            await uow.SaveChangesAsync(cancellationToken);

            await notificationBroadcaster.BroadcastAiChatResponseAsync(
                job.UserId,
                new AiChatResponseSignalREventDto
                {
                    SessionId = session.Id,
                    UserMessageId = userMessage.Id,
                    AiMessageId = aiMessage.Id,
                    Content = aiMessage.Content,
                    CreatedAt = aiMessage.CreatedAt
                },
                cancellationToken);

            await TryUpdateSummaryAsync(uow, aiChatService, session.Id, cancellationToken);

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

        private async Task TryUpdateSummaryAsync(
            IUnitOfWork uow,
            IAiChatService aiChatService,
            int sessionId,
            CancellationToken cancellationToken)
        {
            try
            {
                var session = await uow.ChatSessions.GetByIdAsync(sessionId, cancellationToken);
                if (session == null)
                {
                    return;
                }

                List<ChatMessage> messagesToSummarize;
                if (string.IsNullOrWhiteSpace(session.ConversationSummary) || session.LastSummaryMessageId == null)
                {
                    messagesToSummarize = await uow.ChatMessages.GetAllMessagesBySessionIdAsync(sessionId, cancellationToken);
                }
                else
                {
                    messagesToSummarize = await uow.ChatMessages.GetMessagesAfterIdAsync(
                        sessionId,
                        session.LastSummaryMessageId.Value,
                        cancellationToken);
                }

                if (messagesToSummarize.Count < _aiOptions.RecentMessagesLimit)
                {
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
                var summaryResult = await aiChatService.UpdateSummaryAsync(
                    conversationId,
                    session.ConversationSummary,
                    messagesForSummary,
                    cancellationToken);

                if (!summaryResult.Success || string.IsNullOrWhiteSpace(summaryResult.UpdatedSummary))
                {
                    return;
                }

                session.ConversationSummary = summaryResult.UpdatedSummary;
                session.LastSummaryMessageId = messagesToSummarize[^1].Id;
                session.UpdatedAt = DateTimeOffset.UtcNow;
                uow.ChatSessions.Update(session);
                await uow.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update summary after async chat job for session {SessionId}", sessionId);
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
