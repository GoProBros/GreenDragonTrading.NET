using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Application.UseCases.Chat.Commands.SendChatMessage;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace GreenDragonTrading.Application.Tests.UseCases.Chat.Commands.SendChatMessage;

public class SendChatMessageCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsUnauthenticated_ShouldThrowUnauthenticatedException()
    {
        var fixture = CreateFixture();
        fixture.CurrentUser.SetupGet(x => x.UserId).Returns((Guid?)null);

        var exception = await Assert.ThrowsAsync<UnauthenticatedException>(() => fixture.Handler.Handle(CreateCommand(), CancellationToken.None));

        Assert.Equal("Bạn cần đăng nhập để gửi tin nhắn.", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenSessionNotFound_ShouldThrowNotFoundException()
    {
        var fixture = CreateFixture();

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => fixture.Handler.Handle(CreateCommand(), CancellationToken.None));

        Assert.Equal("Không tìm thấy phiên chat.", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenSessionIsClosed_ShouldThrowBusinessRuleException()
    {
        var fixture = CreateFixture();
        fixture.ChatSessions.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateSession(status: CommonStatus.InActive));

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => fixture.Handler.Handle(CreateCommand(), CancellationToken.None));

        Assert.Equal("Phiên chat đã bị đóng.", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenSessionTypeIsSystem_ShouldThrowBusinessRuleException()
    {
        var fixture = CreateFixture();
        fixture.ChatSessions.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateSession(sessionType: ChatSessionType.System));

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => fixture.Handler.Handle(CreateCommand(), CancellationToken.None));

        Assert.Equal("Không thể gửi tin nhắn người dùng vào phiên System.", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotParticipant_ShouldThrowAccessDeniedException()
    {
        var fixture = CreateFixture();
        fixture.ChatSessions.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateSession());
        fixture.ChatParticipants.Setup(x => x.IsUserParticipantAsync(1, fixture.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var exception = await Assert.ThrowsAsync<AccessDeniedException>(() => fixture.Handler.Handle(CreateCommand(), CancellationToken.None));

        Assert.Equal("Bạn không phải thành viên của phiên chat này.", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenAiAcceptsWithJobId_ShouldEnqueueAsyncJob()
    {
        var fixture = CreateFixture();
        var session = CreateSession();

        fixture.ChatSessions.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        fixture.ChatParticipants.Setup(x => x.IsUserParticipantAsync(1, fixture.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        fixture.ChatMessages.Setup(x => x.GetRecentMessagesAsync(1, It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<ChatMessage>
        {
            new() { Id = 10, SessionId = 1, SenderId = null, Content = "assistant", MessageType = ChatMessageType.Text },
            new() { Id = 11, SessionId = 1, SenderId = fixture.UserId, Content = "user", MessageType = ChatMessageType.Text }
        });
        fixture.AiChatService.Setup(x => x.SubmitMessageAsync("session-1", "Hello AI", It.IsAny<AiChatContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiChatSubmissionResult
            {
                Accepted = true,
                AcceptedResponse = new AiChatAcceptedResponse
                {
                    Accepted = true,
                    JobId = "job-1",
                    Status = ChatAsyncJobStatuses.Queued,
                    PollUrl = string.Empty,
                    IntentHint = "forecast",
                    ConversationId = "session-1"
                }
            });

        ChatAsyncProcessingJob? enqueuedJob = null;
        fixture.ChatAsyncJobService.Setup(x => x.EnqueueAsync(It.IsAny<ChatAsyncProcessingJob>(), It.IsAny<CancellationToken>()))
            .Callback<ChatAsyncProcessingJob, CancellationToken>((job, _) => enqueuedJob = job)
            .Returns(Task.CompletedTask);

        var result = await fixture.Handler.Handle(CreateCommand(message: "Hello AI"), CancellationToken.None);

        Assert.True(result.Data!.Accepted);
        Assert.Equal("job-1", result.Data.JobId);
        Assert.Equal("/api/v1/chat/jobs/job-1", result.Data.PollUrl);
        Assert.Equal(ChatAsyncJobStatuses.Queued, result.Data.Status);
        Assert.NotNull(enqueuedJob);
        Assert.Equal("/api/chat/jobs/job-1", enqueuedJob!.PollUrl);
        Assert.Equal("session-1", enqueuedJob.ConversationId);
        Assert.Equal(1, enqueuedJob.SessionId);
    }

    [Fact]
    public async Task Handle_WhenAiReturnsSyncResponse_ShouldPersistAiMessageAndBroadcast()
    {
        var fixture = CreateFixture(new AiEngineOptions { RecentMessagesLimit = 100 });
        var session = CreateSession();
        var messages = new List<ChatMessage>
        {
            new() { Id = 1, SessionId = 1, SenderId = null, Content = "assistant", MessageType = ChatMessageType.Text },
            new() { Id = 2, SessionId = 1, SenderId = fixture.UserId, Content = "user", MessageType = ChatMessageType.Text }
        };
        ChatMessage? savedUserMessage = null;
        ChatMessage? savedAiMessage = null;

        fixture.ChatSessions.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        fixture.ChatParticipants.Setup(x => x.IsUserParticipantAsync(1, fixture.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        fixture.ChatMessages.Setup(x => x.GetRecentMessagesAsync(1, It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(messages);
        fixture.ChatMessages.Setup(x => x.GetAllMessagesBySessionIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(messages);
        var addCount = 0;
        fixture.ChatMessages.Setup(x => x.AddAsync(It.IsAny<ChatMessage>(), It.IsAny<CancellationToken>()))
            .Callback<ChatMessage, CancellationToken>((message, _) =>
            {
                addCount++;
                if (addCount == 1)
                {
                    savedUserMessage = message;
                }
                else
                {
                    savedAiMessage = message;
                }
            })
            .Returns(Task.CompletedTask);
        fixture.AiChatService.Setup(x => x.SubmitMessageAsync("session-1", "Hello AI", It.IsAny<AiChatContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiChatSubmissionResult
            {
                Accepted = false,
                Response = new AiChatResponse
                {
                    Success = true,
                    ConversationId = "session-1",
                    Response = "AI says hello",
                    ResponseData = new { ok = true },
                    Intents = new List<AiIntentResult> { new() { Intent = "forecast", Confidence = 0.91 } }
                }
            });

        fixture.NotificationBroadcaster.Setup(x => x.BroadcastAiChatResponseAsync(fixture.UserId, It.IsAny<GreenDragonTrading.Application.DTOs.Realtime.AiChatResponseSignalREventDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await fixture.Handler.Handle(CreateCommand(message: "Hello AI"), CancellationToken.None);

        Assert.False(result.Data!.Accepted);
        Assert.Equal(ChatAsyncJobStatuses.Completed, result.Data.Status);
        Assert.NotNull(savedUserMessage);
        Assert.NotNull(savedAiMessage);
        Assert.Equal("AI says hello", result.Data.Result!.AiMessage.Content);
        Assert.Single(result.Data.Result!.Intents!);
        fixture.NotificationBroadcaster.Verify(x => x.BroadcastAiChatResponseAsync(fixture.UserId, It.IsAny<GreenDragonTrading.Application.DTOs.Realtime.AiChatResponseSignalREventDto>(), It.IsAny<CancellationToken>()), Times.Once);
        fixture.ChatMessages.Verify(x => x.AddAsync(It.IsAny<ChatMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_WhenAiAcceptsWithoutJobId_ShouldPersistFallbackResponse()
    {
        var fixture = CreateFixture(new AiEngineOptions { RecentMessagesLimit = 100 });
        var session = CreateSession();

        fixture.ChatSessions.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        fixture.ChatParticipants.Setup(x => x.IsUserParticipantAsync(1, fixture.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        fixture.ChatMessages.Setup(x => x.GetRecentMessagesAsync(1, It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<ChatMessage>());
        fixture.ChatMessages.Setup(x => x.GetAllMessagesBySessionIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new List<ChatMessage>());
        fixture.AiChatService.Setup(x => x.SubmitMessageAsync("session-1", "Hello AI", It.IsAny<AiChatContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiChatSubmissionResult
            {
                Accepted = true,
                AcceptedResponse = new AiChatAcceptedResponse
                {
                    Accepted = true,
                    JobId = string.Empty,
                    Status = ChatAsyncJobStatuses.Queued,
                    ConversationId = "session-1"
                }
            });

        var result = await fixture.Handler.Handle(CreateCommand(message: "Hello AI"), CancellationToken.None);

        Assert.False(result.Data!.Accepted);
        Assert.Equal(ChatAsyncJobStatuses.Completed, result.Data.Status);
        Assert.Equal("Toi chua co du thong tin de tra loi cau hoi nay.", result.Data.Result!.AiMessage.Content);
    }

    [Fact]
    public async Task Handle_WhenAiAcceptsWithCustomPollUrl_ShouldEnqueueUsingReturnedPollUrl()
    {
        var fixture = CreateFixture();
        var session = CreateSession();
        ChatAsyncProcessingJob? enqueuedJob = null;

        fixture.ChatSessions.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        fixture.ChatParticipants.Setup(x => x.IsUserParticipantAsync(1, fixture.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        fixture.AiChatService.Setup(x => x.SubmitMessageAsync("session-1", "Hello AI", It.IsAny<AiChatContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiChatSubmissionResult
            {
                Accepted = true,
                AcceptedResponse = new AiChatAcceptedResponse
                {
                    Accepted = true,
                    JobId = "job-custom",
                    PollUrl = "https://ai.example.com/jobs/job-custom",
                    Status = ChatAsyncJobStatuses.Queued,
                    ConversationId = "session-1"
                }
            });
        fixture.ChatAsyncJobService.Setup(x => x.EnqueueAsync(It.IsAny<ChatAsyncProcessingJob>(), It.IsAny<CancellationToken>()))
            .Callback<ChatAsyncProcessingJob, CancellationToken>((job, _) => enqueuedJob = job)
            .Returns(Task.CompletedTask);

        var result = await fixture.Handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.True(result.Data!.Accepted);
        Assert.NotNull(enqueuedJob);
        Assert.Equal("https://ai.example.com/jobs/job-custom", enqueuedJob!.PollUrl);
        Assert.Equal("/api/v1/chat/jobs/job-custom", result.Data.PollUrl);
    }

    [Fact]
    public async Task Handle_WhenAiAcceptedStatusEmpty_ShouldDefaultToQueued()
    {
        var fixture = CreateFixture();
        var session = CreateSession();

        fixture.ChatSessions.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        fixture.ChatParticipants.Setup(x => x.IsUserParticipantAsync(1, fixture.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        fixture.AiChatService.Setup(x => x.SubmitMessageAsync("session-1", "Hello AI", It.IsAny<AiChatContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiChatSubmissionResult
            {
                Accepted = true,
                AcceptedResponse = new AiChatAcceptedResponse
                {
                    Accepted = true,
                    JobId = "job-empty-status",
                    Status = string.Empty,
                    ConversationId = "session-1"
                }
            });

        var result = await fixture.Handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.True(result.Data!.Accepted);
        Assert.Equal(ChatAsyncJobStatuses.Queued, result.Data.Status);
    }

    private static SendChatMessageCommand CreateCommand(int sessionId = 1, string message = "Hello AI")
    {
        return new SendChatMessageCommand(sessionId, message);
    }

    private static ChatSession CreateSession(
        CommonStatus status = CommonStatus.Active,
        ChatSessionType sessionType = ChatSessionType.AI)
    {
        return new ChatSession
        {
            Id = 1,
            Title = "Chat",
            Status = status,
            SessionType = sessionType,
            ConversationSummary = "summary",
            Participants = new List<ChatParticipant>
            {
                new() { Id = 1, SessionId = 1, UserId = Guid.NewGuid(), Role = ChatRole.Member, User = new User { Username = "Other" } },
                new() { Id = 2, SessionId = 1, UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"), Role = ChatRole.Member, User = new User { Username = "Sender" } }
            }
        };
    }

    private static ChatFixture CreateFixture(AiEngineOptions? aiOptions = null)
    {
        var uow = new Mock<IUnitOfWork>();
        var currentUser = new Mock<ICurrentUserService>();
        var aiChatService = new Mock<IAiChatService>();
        var chatAsyncJobService = new Mock<IChatAsyncJobService>();
        var broadcaster = new Mock<INotificationBroadcaster>();
        var chatSessions = new Mock<IChatSessionRepository>();
        var chatParticipants = new Mock<IChatParticipantRepository>();
        var chatMessages = new Mock<IChatMessageRepository>();

        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        currentUser.SetupGet(x => x.UserId).Returns(userId);

        chatSessions.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((ChatSession?)null);
        chatParticipants.Setup(x => x.IsUserParticipantAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        chatMessages.Setup(x => x.AddAsync(It.IsAny<ChatMessage>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        chatMessages.Setup(x => x.GetRecentMessagesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<ChatMessage>());
        chatMessages.Setup(x => x.GetAllMessagesBySessionIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<ChatMessage>());
        aiChatService.Setup(x => x.SubmitMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AiChatContext>(), It.IsAny<CancellationToken>())).ReturnsAsync(new AiChatSubmissionResult());
        aiChatService.Setup(x => x.UpdateSummaryAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<List<AiMessageInput>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new AiConvSummaryResponse { Success = false });

        uow.SetupGet(x => x.ChatSessions).Returns(chatSessions.Object);
        uow.SetupGet(x => x.ChatParticipants).Returns(chatParticipants.Object);
        uow.SetupGet(x => x.ChatMessages).Returns(chatMessages.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new SendChatMessageCommandHandler(
            uow.Object,
            currentUser.Object,
            aiChatService.Object,
            chatAsyncJobService.Object,
            broadcaster.Object,
            Options.Create(aiOptions ?? new AiEngineOptions { RecentMessagesLimit = 20 }),
            Mock.Of<ILogger<SendChatMessageCommandHandler>>());

        return new ChatFixture(handler, uow, currentUser, aiChatService, chatAsyncJobService, broadcaster, chatSessions, chatParticipants, chatMessages, userId);
    }

    private sealed record ChatFixture(
        SendChatMessageCommandHandler Handler,
        Mock<IUnitOfWork> Uow,
        Mock<ICurrentUserService> CurrentUser,
        Mock<IAiChatService> AiChatService,
        Mock<IChatAsyncJobService> ChatAsyncJobService,
        Mock<INotificationBroadcaster> NotificationBroadcaster,
        Mock<IChatSessionRepository> ChatSessions,
        Mock<IChatParticipantRepository> ChatParticipants,
        Mock<IChatMessageRepository> ChatMessages,
        Guid UserId);
}