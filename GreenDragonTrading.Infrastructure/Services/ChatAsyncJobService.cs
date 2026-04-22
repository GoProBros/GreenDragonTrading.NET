using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace GreenDragonTrading.Infrastructure.Services
{
    public class ChatAsyncJobService : IChatAsyncJobService
    {
        private readonly Channel<ChatAsyncProcessingJob> _jobChannel;
        private readonly ConcurrentDictionary<string, ChatAsyncJobState> _jobStates;
        private readonly TimeSpan _jobTtl;

        public ChatAsyncJobService(IOptions<AiEngineOptions> aiOptions)
        {
            _jobChannel = Channel.CreateUnbounded<ChatAsyncProcessingJob>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
            _jobStates = new ConcurrentDictionary<string, ChatAsyncJobState>(StringComparer.OrdinalIgnoreCase);

            var ttlMinutes = aiOptions.Value.ChatAsyncJobTtlMinutes;
            if (ttlMinutes <= 0)
            {
                ttlMinutes = 30;
            }

            _jobTtl = TimeSpan.FromMinutes(ttlMinutes);
        }

        public async Task EnqueueAsync(ChatAsyncProcessingJob job, CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            var state = new ChatAsyncJobState
            {
                JobId = job.JobId,
                UserId = job.UserId,
                SessionId = job.SessionId,
                UserMessageId = job.UserMessageId,
                Status = ChatAsyncJobStatuses.Queued,
                Success = true,
                Accepted = true,
                CreatedAt = now,
                UpdatedAt = now,
                ExpiresAt = now.Add(_jobTtl)
            };

            _jobStates[job.JobId] = state;
            await _jobChannel.Writer.WriteAsync(job, cancellationToken);
        }

        public ValueTask<ChatAsyncProcessingJob> DequeueAsync(CancellationToken cancellationToken = default)
            => _jobChannel.Reader.ReadAsync(cancellationToken);

        public Task<ChatAsyncJobState?> GetStatusAsync(string jobId, CancellationToken cancellationToken = default)
        {
            if (!_jobStates.TryGetValue(jobId, out var state))
            {
                return Task.FromResult<ChatAsyncJobState?>(null);
            }

            if (state.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                _jobStates.TryRemove(jobId, out _);
                return Task.FromResult<ChatAsyncJobState?>(null);
            }

            return Task.FromResult<ChatAsyncJobState?>(CloneState(state));
        }

        public Task MarkRunningAsync(string jobId, CancellationToken cancellationToken = default)
        {
            if (_jobStates.TryGetValue(jobId, out var state)
                && !string.Equals(state.Status, ChatAsyncJobStatuses.Completed, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(state.Status, ChatAsyncJobStatuses.Failed, StringComparison.OrdinalIgnoreCase))
            {
                var now = DateTimeOffset.UtcNow;
                state.Status = ChatAsyncJobStatuses.Running;
                state.Accepted = true;
                state.Success = true;
                state.UpdatedAt = now;
                state.ExpiresAt = now.Add(_jobTtl);
            }

            return Task.CompletedTask;
        }

        public Task MarkCompletedAsync(string jobId, SendChatMessageResponseDto result, CancellationToken cancellationToken = default)
        {
            if (_jobStates.TryGetValue(jobId, out var state))
            {
                var now = DateTimeOffset.UtcNow;
                state.Status = ChatAsyncJobStatuses.Completed;
                state.Success = true;
                state.Accepted = false;
                state.Error = null;
                state.Result = result;
                state.UpdatedAt = now;
                state.ExpiresAt = now.Add(_jobTtl);
            }

            return Task.CompletedTask;
        }

        public Task MarkFailedAsync(string jobId, string error, CancellationToken cancellationToken = default)
        {
            if (_jobStates.TryGetValue(jobId, out var state))
            {
                var now = DateTimeOffset.UtcNow;
                state.Status = ChatAsyncJobStatuses.Failed;
                state.Success = false;
                state.Accepted = false;
                state.Error = error;
                state.UpdatedAt = now;
                state.ExpiresAt = now.Add(_jobTtl);
            }

            return Task.CompletedTask;
        }

        public Task CleanupExpiredAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            foreach (var item in _jobStates)
            {
                if (item.Value.ExpiresAt <= now)
                {
                    _jobStates.TryRemove(item.Key, out _);
                }
            }

            return Task.CompletedTask;
        }

        private static ChatAsyncJobState CloneState(ChatAsyncJobState state)
        {
            return new ChatAsyncJobState
            {
                JobId = state.JobId,
                UserId = state.UserId,
                SessionId = state.SessionId,
                UserMessageId = state.UserMessageId,
                Status = state.Status,
                Success = state.Success,
                Accepted = state.Accepted,
                Error = state.Error,
                Result = state.Result == null ? null : CloneResult(state.Result),
                CreatedAt = state.CreatedAt,
                UpdatedAt = state.UpdatedAt,
                ExpiresAt = state.ExpiresAt
            };
        }

        private static SendChatMessageResponseDto CloneResult(SendChatMessageResponseDto source)
        {
            return new SendChatMessageResponseDto
            {
                UserMessage = CloneMessage(source.UserMessage),
                AiMessage = CloneMessage(source.AiMessage),
                Intents = source.Intents?.Select(x => new AiIntentDto
                {
                    Intent = x.Intent,
                    Confidence = x.Confidence
                }).ToList()
            };
        }

        private static ChatMessageDto CloneMessage(ChatMessageDto source)
        {
            return new ChatMessageDto
            {
                Id = source.Id,
                SessionId = source.SessionId,
                SenderId = source.SenderId,
                Content = source.Content,
                ResponseData = source.ResponseData,
                MessageType = source.MessageType,
                FileUrl = source.FileUrl,
                FileName = source.FileName,
                FileSize = source.FileSize,
                CreatedAt = source.CreatedAt,
                UpdatedAt = source.UpdatedAt
            };
        }
    }
}
