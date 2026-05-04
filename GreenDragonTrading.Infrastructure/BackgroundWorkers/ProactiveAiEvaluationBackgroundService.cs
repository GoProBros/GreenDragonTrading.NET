using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.DTOs.Realtime;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Infrastructure.BackgroundWorkers
{
    /// <summary>
    /// Consumes proactive AI evaluation jobs from Redis queue and calls AI service asynchronously.
    /// </summary>
    public class ProactiveAiEvaluationBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ProactiveAiEvaluationBackgroundService> logger) : BackgroundService
    {
        private const int MaxBatchSize = 20;
        private static readonly TimeSpan EmptyQueueDelay = TimeSpan.FromMilliseconds(600);

        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
        private readonly ILogger<ProactiveAiEvaluationBackgroundService> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Proactive AI evaluation worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var redis = scope.ServiceProvider.GetRequiredService<IRedisService>();
                    var jobs = await DequeueBatchAsync(redis);

                    if (jobs.Count == 0)
                    {
                        await Task.Delay(EmptyQueueDelay, stoppingToken);
                        continue;
                    }

                    foreach (var job in jobs)
                    {
                        if (stoppingToken.IsCancellationRequested)
                        {
                            break;
                        }

                        await ProcessJobAsync(scope.ServiceProvider, job, stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in proactive AI evaluation worker loop.");
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
            }

            _logger.LogInformation("Proactive AI evaluation worker stopped.");
        }

        private static async Task<List<ProactiveAiEvaluationJobDto>> DequeueBatchAsync(IRedisService redis)
        {
            var jobs = new List<ProactiveAiEvaluationJobDto>(MaxBatchSize);
            var queueKey = RedisConstants.ProactiveAiEvaluationQueue();

            for (var i = 0; i < MaxBatchSize; i++)
            {
                var job = await redis.ListLeftPopAsync<ProactiveAiEvaluationJobDto>(queueKey);
                if (job == null)
                {
                    break;
                }

                jobs.Add(job);
            }

            return jobs;
        }

        private async Task ProcessJobAsync(
            IServiceProvider serviceProvider,
            ProactiveAiEvaluationJobDto job,
            CancellationToken cancellationToken)
        {
            if (job.TargetUserIds.Count == 0)
            {
                return;
            }

            var uow = serviceProvider.GetRequiredService<IUnitOfWork>();
            var proactiveEvaluationService = serviceProvider.GetRequiredService<IProactiveAlertEvaluationService>();
            var notificationBroadcaster = serviceProvider.GetRequiredService<INotificationBroadcaster>();

            try
            {
                var activeUsers = (await uow.Users.FindAsync(
                    x => job.TargetUserIds.Contains(x.Id) && x.Status == CommonStatus.Active,
                    cancellationToken)).ToList();

                if (activeUsers.Count == 0)
                {
                    return;
                }

                var aiResult = await EvaluateWithAiAsync(proactiveEvaluationService, job, cancellationToken);
                if (!aiResult.IsSuccess)
                {
                    return;
                }

                var now = DateTimeOffset.UtcNow;
                var pendingBroadcasts = new List<PendingBroadcast>(activeUsers.Count);

                foreach (var user in activeUsers)
                {
                    var systemSessionId = await GetOrCreateSystemSessionIdAsync(uow, user.Id, now, cancellationToken);
                    var savedMessage = await SaveAiEvaluationMessageAsync(
                        uow,
                        systemSessionId,
                        aiResult,
                        now,
                        cancellationToken);

                    pendingBroadcasts.Add(new PendingBroadcast
                    {
                        UserId = user.Id,
                        SessionId = systemSessionId,
                        Message = savedMessage,
                    });
                }

                if (pendingBroadcasts.Count == 0)
                {
                    return;
                }

                await uow.SaveChangesAsync(cancellationToken);

                foreach (var pending in pendingBroadcasts)
                {
                    try
                    {
                        await notificationBroadcaster.BroadcastSystemChatMessageAsync(
                            pending.UserId,
                            new SystemChatMessageSignalREventDto
                            {
                                SessionId = pending.SessionId,
                                MessageId = pending.Message.Id,
                                MessageType = pending.Message.MessageType.ToString(),
                                Source = "ProactiveAiEvaluated",
                                Content = pending.Message.Content,
                                CreatedAt = pending.Message.CreatedAt,
                                AlertId = null,
                                Ticker = job.Ticker,
                            },
                            cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "Failed broadcasting proactive AI message for user {UserId} ticker {Ticker}",
                            pending.UserId,
                            job.Ticker);
                    }
                }

                _logger.LogInformation(
                    "Processed proactive AI job {JobId} for ticker {Ticker} and {UserCount} users",
                    job.JobId,
                    job.Ticker,
                    pendingBroadcasts.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed processing proactive AI job {JobId} for ticker {Ticker}",
                    job.JobId,
                    job.Ticker);
            }
        }

        private async Task<AiEvaluationResult> EvaluateWithAiAsync(
            IProactiveAlertEvaluationService proactiveEvaluationService,
            ProactiveAiEvaluationJobDto job,
            CancellationToken cancellationToken)
        {
            try
            {
                var evaluationResult = await proactiveEvaluationService.EvaluateAsync(job, cancellationToken);
                if (!evaluationResult.Success || evaluationResult.Data == null)
                {
                    if (evaluationResult.IsValidationError)
                    {
                        _logger.LogWarning(
                            "Proactive evaluation request invalid for job {JobId} ticker {Ticker}. ErrorCode: {ErrorCode}, Message: {Message}",
                            job.JobId,
                            job.Ticker,
                            evaluationResult.ErrorCode,
                            evaluationResult.ErrorMessage);
                    }
                    else if (evaluationResult.IsConflict)
                    {
                        _logger.LogWarning(
                            "Proactive evaluation conflict for job {JobId} ticker {Ticker}. ErrorCode: {ErrorCode}, Message: {Message}",
                            job.JobId,
                            job.Ticker,
                            evaluationResult.ErrorCode,
                            evaluationResult.ErrorMessage);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "AI proactive evaluation failed for job {JobId} ticker {Ticker}. ErrorCode: {ErrorCode}, Message: {Message}",
                            job.JobId,
                            job.Ticker,
                            evaluationResult.ErrorCode,
                            evaluationResult.ErrorMessage);
                    }

                    return AiEvaluationResult.Failure();
                }

                var content = !string.IsNullOrWhiteSpace(evaluationResult.Data.UserMessage)
                    ? evaluationResult.Data.UserMessage
                    : BuildFallbackProactiveSummary(job);

                return new AiEvaluationResult
                {
                    IsSuccess = true,
                    Content = content,
                    ResponseDataJson = System.Text.Json.JsonSerializer.Serialize(evaluationResult.Data),
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calling AI proactive evaluation for {Ticker}", job.Ticker);
                return AiEvaluationResult.Failure();
            }
        }

        private static string BuildFallbackProactiveSummary(ProactiveAiEvaluationJobDto job)
        {
            return string.Format(
                "AI evaluation fallback for {0}: move {1:+0.##;-0.##}% (threshold {2:0.##}%), volume/MA20 {3:0.##}, ADX14 {4:0.##}.",
                job.Ticker,
                job.SignedMovePercent,
                job.RequiredMovePercent,
                job.VolumeRatio,
                job.Adx14);
        }

        private static async Task<int> GetOrCreateSystemSessionIdAsync(
            IUnitOfWork uow,
            Guid userId,
            DateTimeOffset now,
            CancellationToken cancellationToken)
        {
            var systemSession = await uow.ChatSessions.GetSystemSessionByUserIdAsync(userId, cancellationToken);

            if (systemSession == null)
            {
                systemSession = new ChatSession
                {
                    Title = ChatConstants.SystemNotificationSessionTitle,
                    SessionType = ChatSessionType.System,
                    Status = CommonStatus.Active,
                    CreatedBy = null,
                    CreatedAt = now,
                    UpdatedAt = now,
                };

                await uow.ChatSessions.AddAsync(systemSession, cancellationToken);
                await uow.SaveChangesAsync(cancellationToken);

                await uow.ChatParticipants.AddAsync(new ChatParticipant
                {
                    SessionId = systemSession.Id,
                    UserId = userId,
                    Role = ChatRole.Member,
                    JoinedAt = now,
                    LastReadAt = now,
                }, cancellationToken);

                return systemSession.Id;
            }

            var isParticipant = await uow.ChatParticipants.IsUserParticipantAsync(systemSession.Id, userId, cancellationToken);
            if (!isParticipant)
            {
                await uow.ChatParticipants.AddAsync(new ChatParticipant
                {
                    SessionId = systemSession.Id,
                    UserId = userId,
                    Role = ChatRole.Member,
                    JoinedAt = now,
                    LastReadAt = now,
                }, cancellationToken);
            }

            systemSession.UpdatedAt = now;
            uow.ChatSessions.Update(systemSession);

            return systemSession.Id;
        }

        private static async Task<ChatMessage> SaveAiEvaluationMessageAsync(
            IUnitOfWork uow,
            int systemSessionId,
            AiEvaluationResult aiEvaluationResult,
            DateTimeOffset now,
            CancellationToken cancellationToken)
        {
            var chatMessage = new ChatMessage
            {
                SessionId = systemSessionId,
                SenderId = null,
                Content = aiEvaluationResult.Content,
                ResponseData = aiEvaluationResult.ResponseDataJson,
                MessageType = ChatMessageType.SystemNotification,
                CreatedAt = now,
                UpdatedAt = now,
            };

            await uow.ChatMessages.AddAsync(chatMessage, cancellationToken);
            return chatMessage;
        }

        private sealed class AiEvaluationResult
        {
            public bool IsSuccess { get; init; }
            public string Content { get; init; } = string.Empty;
            public string? ResponseDataJson { get; init; }

            public static AiEvaluationResult Failure() => new() { IsSuccess = false };
        }

        private sealed class PendingBroadcast
        {
            public Guid UserId { get; init; }
            public int SessionId { get; init; }
            public ChatMessage Message { get; init; } = default!;
        }
    }
}
