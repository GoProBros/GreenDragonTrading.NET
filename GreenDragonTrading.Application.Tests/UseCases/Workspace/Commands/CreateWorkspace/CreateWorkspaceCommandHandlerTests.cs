using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Application.UseCases.Workspace.Commands.CreateWorkspace;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Text.Json;
using WorkspaceEntity = GreenDragonTrading.Domain.Entities.Workspace;

namespace GreenDragonTrading.Application.Tests.UseCases.Workspace.Commands.CreateWorkspace;

public class CreateWorkspaceCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenNonAdminExceedsWorkspaceLimit_ShouldThrowBusinessRuleException()
    {
        var fixture = CreateFixture(isAdminOrStaff: false);
        fixture.UserSubscriptions.Setup(x => x.GetActiveSubscriptionAsync(fixture.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new UserSubscription
        {
            Subscription = new Subscription { Id = 1, MaxWorkspaces = 1, LevelOrder = SubscriptionLevel.Free }
        });
        fixture.Workspaces.Setup(x => x.GetWorkspaceByUserIdAsync(fixture.UserId, WorkspaceType.Web, It.IsAny<CancellationToken>())).ReturnsAsync(new List<WorkspaceEntity> { new() { Id = 1 } });

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => fixture.Handler.Handle(CreateCommand(), CancellationToken.None));

        Assert.Contains("tối đa 1 workspace", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenNonAdminUnderLimit_ShouldCreateWorkspaceFromSystemDefaultLayout()
    {
        var fixture = CreateFixture(isAdminOrStaff: false);
        WorkspaceEntity? savedWorkspace = null;

        fixture.UserSubscriptions.Setup(x => x.GetActiveSubscriptionAsync(fixture.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new UserSubscription
        {
            Subscription = new Subscription { Id = 1, MaxWorkspaces = 3, LevelOrder = SubscriptionLevel.Free }
        });
        fixture.Workspaces.Setup(x => x.GetWorkspaceByUserIdAsync(fixture.UserId, WorkspaceType.Web, It.IsAny<CancellationToken>())).ReturnsAsync(new List<WorkspaceEntity> { new() { Id = 1 } });
        fixture.Workspaces.Setup(x => x.ShareCodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        fixture.Workspaces.Setup(x => x.GetSystemDefaultWorkspaceAsync(WorkspaceType.Web, It.IsAny<CancellationToken>())).ReturnsAsync(new WorkspaceEntity { Id = 2, LayoutJson = "{\"grid\":true}", Type = WorkspaceType.Web, IsDefault = true });
        fixture.Workspaces.Setup(x => x.AddAsync(It.IsAny<WorkspaceEntity>(), It.IsAny<CancellationToken>()))
            .Callback<WorkspaceEntity, CancellationToken>((workspace, _) => savedWorkspace = workspace)
            .Returns(Task.CompletedTask);

        var result = await fixture.Handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(savedWorkspace);
        Assert.Equal("My workspace", savedWorkspace!.WorkspaceName);
        Assert.Equal(WorkspaceType.Web, savedWorkspace.Type);
        Assert.False(string.IsNullOrWhiteSpace(savedWorkspace.ShareCode));
        Assert.Equal(8, savedWorkspace.ShareCode!.Length);
        Assert.True(result.Data!.LayoutJson!.Value.GetProperty("grid").GetBoolean());
    }

    [Fact]
    public async Task Handle_WhenAdmin_ShouldBypassWorkspaceLimitCheck()
    {
        var fixture = CreateFixture(isAdminOrStaff: true);
        fixture.Workspaces.Setup(x => x.ShareCodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        fixture.Workspaces.Setup(x => x.GetSystemDefaultWorkspaceAsync(WorkspaceType.Web, It.IsAny<CancellationToken>())).ReturnsAsync(new WorkspaceEntity { Id = 2, LayoutJson = "{}", Type = WorkspaceType.Web, IsDefault = true });
        fixture.Workspaces.Setup(x => x.AddAsync(It.IsAny<WorkspaceEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await fixture.Handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        fixture.UserSubscriptions.Verify(x => x.GetActiveSubscriptionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenShareCodeCollides_ShouldRetryUntilUnique()
    {
        var fixture = CreateFixture(isAdminOrStaff: true);
        fixture.Workspaces.SetupSequence(x => x.ShareCodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);
        fixture.Workspaces.Setup(x => x.GetSystemDefaultWorkspaceAsync(WorkspaceType.Web, It.IsAny<CancellationToken>())).ReturnsAsync(new WorkspaceEntity { Id = 2, LayoutJson = "{}", Type = WorkspaceType.Web, IsDefault = true });
        fixture.Workspaces.Setup(x => x.AddAsync(It.IsAny<WorkspaceEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await fixture.Handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        fixture.Workspaces.Verify(x => x.ShareCodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_WhenShareCodeCannotBeGenerated_ShouldThrowInvalidOperationException()
    {
        var fixture = CreateFixture(isAdminOrStaff: true);
        fixture.Workspaces.Setup(x => x.ShareCodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Handler.Handle(CreateCommand(), CancellationToken.None));

        Assert.Contains("không thể tạo mã chia sẻ duy nhất", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenNonAdminWithoutActiveSubscription_ShouldSkipWorkspaceLimitValidation()
    {
        var fixture = CreateFixture(isAdminOrStaff: false);
        fixture.UserSubscriptions.Setup(x => x.GetActiveSubscriptionAsync(fixture.UserId, It.IsAny<CancellationToken>())).ReturnsAsync((UserSubscription?)null);
        fixture.Workspaces.Setup(x => x.ShareCodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        fixture.Workspaces.Setup(x => x.GetSystemDefaultWorkspaceAsync(WorkspaceType.Web, It.IsAny<CancellationToken>())).ReturnsAsync(new WorkspaceEntity { Id = 10, LayoutJson = "{}", Type = WorkspaceType.Web, IsDefault = true });

        var result = await fixture.Handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        fixture.Workspaces.Verify(x => x.GetWorkspaceByUserIdAsync(It.IsAny<Guid>(), It.IsAny<WorkspaceType>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNoSystemDefaultLayout_ShouldUseEmptyJsonObject()
    {
        var fixture = CreateFixture(isAdminOrStaff: true);
        fixture.Workspaces.Setup(x => x.GetSystemDefaultWorkspaceAsync(WorkspaceType.Web, It.IsAny<CancellationToken>())).ReturnsAsync((WorkspaceEntity?)null);

        var result = await fixture.Handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data!.LayoutJson);
        Assert.Equal(JsonValueKind.Object, result.Data.LayoutJson!.Value.ValueKind);
        Assert.Empty(result.Data.LayoutJson.Value.EnumerateObject());
    }

    [Fact]
    public async Task Handle_WhenWorkspaceCreated_ShouldPersistAndSaveOnce()
    {
        var fixture = CreateFixture(isAdminOrStaff: true);

        await fixture.Handler.Handle(CreateCommand(), CancellationToken.None);

        fixture.Workspaces.Verify(x => x.AddAsync(It.IsAny<WorkspaceEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        fixture.Uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenWorkspaceCreated_ShouldReturnRequestNameAndDefaultFlag()
    {
        var fixture = CreateFixture(isAdminOrStaff: true);
        fixture.Workspaces.Setup(x => x.GetSystemDefaultWorkspaceAsync(WorkspaceType.Web, It.IsAny<CancellationToken>())).ReturnsAsync(new WorkspaceEntity { Id = 2, LayoutJson = "{}", Type = WorkspaceType.Web, IsDefault = true });

        var result = await fixture.Handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.Equal("My workspace", result.Data!.WorkspaceName);
        Assert.True(result.Data.IsDefault);
        Assert.Equal(WorkspaceType.Web, result.Data.Type);
    }

    [Fact]
    public async Task Handle_WhenNonAdminWithSubscription_ShouldQueryWorkspacesUsingRequestedType()
    {
        var fixture = CreateFixture(isAdminOrStaff: false);
        fixture.UserSubscriptions.Setup(x => x.GetActiveSubscriptionAsync(fixture.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new UserSubscription
        {
            Subscription = new Subscription { Id = 1, MaxWorkspaces = 5, LevelOrder = SubscriptionLevel.Free }
        });
        fixture.Workspaces.Setup(x => x.GetWorkspaceByUserIdAsync(fixture.UserId, WorkspaceType.Web, It.IsAny<CancellationToken>())).ReturnsAsync(new List<WorkspaceEntity>());

        var result = await fixture.Handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        fixture.Workspaces.Verify(x => x.GetWorkspaceByUserIdAsync(fixture.UserId, WorkspaceType.Web, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static CreateWorkspaceCommand CreateCommand()
    {
        return new CreateWorkspaceCommand(
            "My workspace",
            JsonDocument.Parse("{\"client\":true}").RootElement,
            WorkspaceType.Web,
            true);
    }

    private static WorkspaceFixture CreateFixture(bool isAdminOrStaff)
    {
        var uow = new Mock<IUnitOfWork>();
        var currentUser = new Mock<ICurrentUserService>();
        var userSubscriptions = new Mock<IUserSubscriptionRepository>();
        var workspaces = new Mock<IWorkspaceRepository>();
        var userId = Guid.NewGuid();

        currentUser.SetupGet(x => x.IsAdminOrStaff).Returns(isAdminOrStaff);
        currentUser.Setup(x => x.GetRequiredUserId()).Returns(userId);
        userSubscriptions.Setup(x => x.GetActiveSubscriptionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((UserSubscription?)null);
        workspaces.Setup(x => x.GetWorkspaceByUserIdAsync(It.IsAny<Guid>(), It.IsAny<WorkspaceType>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<WorkspaceEntity>());
        workspaces.Setup(x => x.GetSystemDefaultWorkspaceAsync(It.IsAny<WorkspaceType>(), It.IsAny<CancellationToken>())).ReturnsAsync((WorkspaceEntity?)null);
        workspaces.Setup(x => x.ShareCodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        workspaces.Setup(x => x.AddAsync(It.IsAny<WorkspaceEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        uow.SetupGet(x => x.UserSubscriptions).Returns(userSubscriptions.Object);
        uow.SetupGet(x => x.Workspaces).Returns(workspaces.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateWorkspaceCommandHandler(
            uow.Object,
            currentUser.Object,
            Mock.Of<ILogger<CreateWorkspaceCommandHandler>>());

        return new WorkspaceFixture(handler, uow, currentUser, userSubscriptions, workspaces, userId);
    }

    private sealed record WorkspaceFixture(
        CreateWorkspaceCommandHandler Handler,
        Mock<IUnitOfWork> Uow,
        Mock<ICurrentUserService> CurrentUser,
        Mock<IUserSubscriptionRepository> UserSubscriptions,
        Mock<IWorkspaceRepository> Workspaces,
        Guid UserId);
}