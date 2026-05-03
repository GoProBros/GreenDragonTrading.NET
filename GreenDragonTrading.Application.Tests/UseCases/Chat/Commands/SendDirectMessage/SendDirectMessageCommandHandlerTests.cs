using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.DTOs.Realtime;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Application.UseCases.Chat.Commands.SendDirectMessage;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GreenDragonTrading.Application.Tests.UseCases.Chat.Commands.SendDirectMessage;

public class SendDirectMessageCommandHandlerTests
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

        Assert.Equal("Không tìm thấy phiên trò chuyện.", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenSessionIsNotDirect_ShouldThrowBusinessRuleException()
    {
        var fixture = CreateFixture();
        fixture.ChatSessions.Setup(x => x.GetSessionWithMessagesAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateSession(sessionType: ChatSessionType.AI));

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => fixture.Handler.Handle(CreateCommand(), CancellationToken.None));

        Assert.Equal("Endpoint này chỉ dành cho phiên trò chuyện trực tiếp.", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenSessionIsClosed_ShouldThrowBusinessRuleException()
    {
        var fixture = CreateFixture();
        fixture.ChatSessions.Setup(x => x.GetSessionWithMessagesAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateSession(status: CommonStatus.InActive));

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => fixture.Handler.Handle(CreateCommand(), CancellationToken.None));

        Assert.Equal("Phiên trò chuyện đã bị đóng.", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenUserNotParticipant_ShouldThrowAccessDeniedException()
    {
        var fixture = CreateFixture();
        fixture.ChatSessions.Setup(x => x.GetSessionWithMessagesAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateSession(includeSenderParticipant: false));

        var exception = await Assert.ThrowsAsync<AccessDeniedException>(() => fixture.Handler.Handle(CreateCommand(), CancellationToken.None));

        Assert.Equal("Bạn không phải là thành viên của cuộc trò chuyện này.", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenValidDirectMessage_ShouldPersistAndBroadcastToRecipients()
    {
        var fixture = CreateFixture();
        var session = CreateSession();
        ChatMessage? savedMessage = null;
        Guid broadcastRecipientId = Guid.Empty;
        DirectMessageSignalREventDto? broadcastPayload = null;

        fixture.ChatSessions.Setup(x => x.GetSessionWithMessagesAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        fixture.ChatMessages.Setup(x => x.AddAsync(It.IsAny<ChatMessage>(), It.IsAny<CancellationToken>()))
            .Callback<ChatMessage, CancellationToken>((message, _) =>
            {
                message.Id = 99;
                savedMessage = message;
            })
            .Returns(Task.CompletedTask);
        fixture.NotificationBroadcaster.Setup(x => x.BroadcastDirectMessageAsync(It.IsAny<Guid>(), It.IsAny<DirectMessageSignalREventDto>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, DirectMessageSignalREventDto, CancellationToken>((recipientId, payload, _) =>
            {
                broadcastRecipientId = recipientId;
                broadcastPayload = payload;
            })
            .Returns(Task.CompletedTask);

        var result = await fixture.Handler.Handle(CreateCommand(content: "Hello direct"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(savedMessage);
        Assert.Equal(99, result.Data!.Id);
        Assert.Equal("Sender", result.Data.SenderName);
        Assert.NotEqual(Guid.Empty, broadcastRecipientId);
        Assert.NotNull(broadcastPayload);
        Assert.Equal("Hello direct", broadcastPayload!.Content);
        fixture.NotificationBroadcaster.Verify(x => x.BroadcastDirectMessageAsync(It.IsAny<Guid>(), It.IsAny<DirectMessageSignalREventDto>(), It.IsAny<CancellationToken>()), Times.Once);
        fixture.ChatParticipants.Verify(x => x.Update(It.IsAny<ChatParticipant>()), Times.Once);
        fixture.ChatSessions.Verify(x => x.Update(It.IsAny<ChatSession>()), Times.Once);
        fixture.Uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_WhenNoOtherParticipants_ShouldNotBroadcast()
    {
        var fixture = CreateFixture();
        var session = CreateSession();
        session.Participants = session.Participants.Where(x => x.UserId == fixture.UserId).ToList();

        fixture.ChatSessions.Setup(x => x.GetSessionWithMessagesAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        var result = await fixture.Handler.Handle(CreateCommand(content: "Self message"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        fixture.NotificationBroadcaster.Verify(x => x.BroadcastDirectMessageAsync(It.IsAny<Guid>(), It.IsAny<DirectMessageSignalREventDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSenderUsernameMissing_ShouldFallbackToUserIdAsSenderName()
    {
        var fixture = CreateFixture();
        var session = CreateSession();
        var sender = session.Participants.Single(x => x.UserId == fixture.UserId);
        sender.User = null!;

        fixture.ChatSessions.Setup(x => x.GetSessionWithMessagesAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        var result = await fixture.Handler.Handle(CreateCommand(content: "Fallback sender"), CancellationToken.None);

        Assert.Equal(fixture.UserId.ToString(), result.Data!.SenderName);
    }

    [Fact]
    public async Task Handle_WhenRecipientIdsContainDuplicates_ShouldBroadcastDistinctRecipientsOnly()
    {
        var fixture = CreateFixture();
        var duplicateRecipientId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var anotherRecipientId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var session = new ChatSession
        {
            Id = 1,
            Title = "Direct",
            SessionType = ChatSessionType.Direct,
            Status = CommonStatus.Active,
            Participants = new List<ChatParticipant>
            {
                new() { Id = 1, SessionId = 1, UserId = fixture.UserId, Role = ChatRole.Member, User = new User { Username = "Sender" } },
                new() { Id = 2, SessionId = 1, UserId = duplicateRecipientId, Role = ChatRole.Member, User = new User { Username = "R1" } },
                new() { Id = 3, SessionId = 1, UserId = duplicateRecipientId, Role = ChatRole.Member, User = new User { Username = "R1 duplicate" } },
                new() { Id = 4, SessionId = 1, UserId = anotherRecipientId, Role = ChatRole.Member, User = new User { Username = "R2" } }
            }
        };

        var recipients = new List<Guid>();
        fixture.ChatSessions.Setup(x => x.GetSessionWithMessagesAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        fixture.NotificationBroadcaster.Setup(x => x.BroadcastDirectMessageAsync(It.IsAny<Guid>(), It.IsAny<DirectMessageSignalREventDto>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, DirectMessageSignalREventDto, CancellationToken>((recipientId, _, _) => recipients.Add(recipientId))
            .Returns(Task.CompletedTask);

        await fixture.Handler.Handle(CreateCommand(content: "Hello all"), CancellationToken.None);

        Assert.Equal(2, recipients.Count);
        Assert.Contains(duplicateRecipientId, recipients);
        Assert.Contains(anotherRecipientId, recipients);
    }

    [Fact]
    public async Task Handle_WhenMessageSaved_ShouldUpdateSenderReadStateWithMessageId()
    {
        var fixture = CreateFixture();
        var session = CreateSession();
        ChatParticipant? updatedParticipant = null;

        fixture.ChatSessions.Setup(x => x.GetSessionWithMessagesAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        fixture.ChatMessages.Setup(x => x.AddAsync(It.IsAny<ChatMessage>(), It.IsAny<CancellationToken>()))
            .Callback<ChatMessage, CancellationToken>((message, _) => message.Id = 321)
            .Returns(Task.CompletedTask);
        fixture.ChatParticipants.Setup(x => x.Update(It.IsAny<ChatParticipant>()))
            .Callback<ChatParticipant>(participant => updatedParticipant = participant);

        await fixture.Handler.Handle(CreateCommand(content: "Read tracking"), CancellationToken.None);

        Assert.NotNull(updatedParticipant);
        Assert.Equal(321, updatedParticipant!.LastReadMessageId);
        Assert.NotEqual(default, updatedParticipant.LastReadAt);
    }

    private static SendDirectMessageCommand CreateCommand(int sessionId = 1, string content = "Hello direct")
    {
        return new SendDirectMessageCommand(sessionId, content);
    }

    private static ChatSession CreateSession(
        CommonStatus status = CommonStatus.Active,
        ChatSessionType sessionType = ChatSessionType.Direct,
        bool includeSenderParticipant = true)
    {
        var senderId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var participants = new List<ChatParticipant>
        {
            new() { Id = 1, SessionId = 1, UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"), Role = ChatRole.Member, User = new User { Username = "Recipient" } }
        };

        if (includeSenderParticipant)
        {
            participants.Add(new ChatParticipant { Id = 2, SessionId = 1, UserId = senderId, Role = ChatRole.Member, User = new User { Username = "Sender" } });
        }

        return new ChatSession
        {
            Id = 1,
            Title = "Direct",
            SessionType = sessionType,
            Status = status,
            Participants = participants
        };
    }

    private static DirectMessageFixture CreateFixture()
    {
        var uow = new Mock<IUnitOfWork>();
        var currentUser = new Mock<ICurrentUserService>();
        var notificationBroadcaster = new Mock<INotificationBroadcaster>();
        var chatSessions = new Mock<IChatSessionRepository>();
        var chatParticipants = new Mock<IChatParticipantRepository>();
        var chatMessages = new Mock<IChatMessageRepository>();

        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        currentUser.SetupGet(x => x.UserId).Returns(userId);

        chatSessions.Setup(x => x.GetSessionWithMessagesAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((ChatSession?)null);
        chatMessages.Setup(x => x.AddAsync(It.IsAny<ChatMessage>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        chatParticipants.Setup(x => x.Update(It.IsAny<ChatParticipant>()));
        chatSessions.Setup(x => x.Update(It.IsAny<ChatSession>()));

        uow.SetupGet(x => x.ChatSessions).Returns(chatSessions.Object);
        uow.SetupGet(x => x.ChatParticipants).Returns(chatParticipants.Object);
        uow.SetupGet(x => x.ChatMessages).Returns(chatMessages.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new SendDirectMessageCommandHandler(
            uow.Object,
            currentUser.Object,
            notificationBroadcaster.Object,
            Mock.Of<ILogger<SendDirectMessageCommandHandler>>());

        return new DirectMessageFixture(handler, uow, currentUser, notificationBroadcaster, chatSessions, chatParticipants, chatMessages, userId);
    }

    private sealed record DirectMessageFixture(
        SendDirectMessageCommandHandler Handler,
        Mock<IUnitOfWork> Uow,
        Mock<ICurrentUserService> CurrentUser,
        Mock<INotificationBroadcaster> NotificationBroadcaster,
        Mock<IChatSessionRepository> ChatSessions,
        Mock<IChatParticipantRepository> ChatParticipants,
        Mock<IChatMessageRepository> ChatMessages,
        Guid UserId);
}