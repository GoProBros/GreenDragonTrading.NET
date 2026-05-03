using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Application.UseCases.Auth.Commands.Register;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using WorkspaceEntity = GreenDragonTrading.Domain.Entities.Workspace;

namespace GreenDragonTrading.Application.Tests.UseCases.Auth.Commands.Register;

public class RegisterCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenEmailExists_ShouldThrowConflictException()
    {
        var fixture = CreateFixture();
        fixture.Users.Setup(x => x.EmailExistsAsync("new@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<ConflictException>(() => fixture.Handler.Handle(CreateCommand(), CancellationToken.None));

        Assert.Equal("Email này đã được đăng kí.", exception.Message);
        fixture.Users.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        fixture.Email.Verify(x => x.SendVerificationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPhoneExists_ShouldThrowConflictException()
    {
        var fixture = CreateFixture();
        fixture.Users.Setup(x => x.PhoneNumberExistsAsync("0912345678", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<ConflictException>(() => fixture.Handler.Handle(CreateCommand(), CancellationToken.None));

        Assert.Equal("Số điện thoại này đã được đăng kí.", exception.Message);
        fixture.Users.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    
    [Fact]
    public async Task Handle_WhenVerificationUrlMissing_ShouldUseBaseUrlFallback()
    {
        var fixture = CreateFixture(new Dictionary<string, string?>
        {
            ["AppSettings:BaseUrl"] = "https://app.example.com"
        });

        string? verificationUrl = null;

        fixture.Users
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        fixture.Workspaces
            .Setup(x => x.GetSystemDefaultWorkspaceAsync(It.IsAny<WorkspaceType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkspaceEntity?)null);
        fixture.Email
            .Setup(x => x.SendVerificationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, string, CancellationToken>((_, _, url, _) => verificationUrl = url)
            .Returns(Task.CompletedTask);

        var result = await fixture.Handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("https://app.example.com/api/v1/auth/verify-email?token={token}", verificationUrl);
    }

    [Fact]
    public async Task Handle_WhenOneDefaultWorkspaceMissing_ShouldContinueRegistration()
    {
        var fixture = CreateFixture();
        var duplicatedWorkspaceTypes = new List<WorkspaceType>();

        fixture.Users
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        fixture.Workspaces
            .SetupSequence(x => x.GetSystemDefaultWorkspaceAsync(It.IsAny<WorkspaceType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkspaceEntity { Id = 1, Type = WorkspaceType.Web, LayoutJson = "{}" })
            .ReturnsAsync((WorkspaceEntity?)null);

        fixture.WorkspaceDuplicationService
            .Setup(x => x.DuplicateWorkspaceAsync(It.IsAny<WorkspaceEntity>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<WorkspaceEntity, Guid, string, CancellationToken>((workspace, _, _, _) => duplicatedWorkspaceTypes.Add(workspace.Type))
            .ReturnsAsync((WorkspaceEntity sourceWorkspace, Guid targetUserId, string suffix, CancellationToken _) => new WorkspaceEntity
            {
                Id = sourceWorkspace.Id + 100,
                UserId = targetUserId,
                WorkspaceName = sourceWorkspace.WorkspaceName + suffix,
                LayoutJson = sourceWorkspace.LayoutJson,
                Type = sourceWorkspace.Type,
                IsDefault = sourceWorkspace.IsDefault,
                ShareCode = "ABCDEFGH"
            });

        var result = await fixture.Handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(duplicatedWorkspaceTypes);
        Assert.Contains(WorkspaceType.Web, duplicatedWorkspaceTypes);
    }

    [Fact]
    public async Task Handle_WhenVerificationUrlConfigured_ShouldStoreTwoRedisKeysAndSendEmail()
    {
        var fixture = CreateFixture(new Dictionary<string, string?>
        {
            ["AppSettings:EmailVerificationUrl"] = "https://verify.example.com?token={token}"
        });

        fixture.Workspaces
            .Setup(x => x.GetSystemDefaultWorkspaceAsync(It.IsAny<WorkspaceType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkspaceEntity?)null);

        var result = await fixture.Handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        fixture.Redis.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<TimeSpan?>()), Times.Once);
        fixture.Redis.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>()), Times.Once);
        fixture.Email.Verify(x => x.SendVerificationEmailAsync("new@example.com", It.IsAny<string>(), "https://verify.example.com?token={token}", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldTrimPhoneBeforePhoneDuplicateCheck()
    {
        var fixture = CreateFixture();
        fixture.Workspaces
            .Setup(x => x.GetSystemDefaultWorkspaceAsync(It.IsAny<WorkspaceType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkspaceEntity?)null);

        await fixture.Handler.Handle(CreateCommand(phoneNumber: " 0912345678 "), CancellationToken.None);

        fixture.Users.Verify(x => x.PhoneNumberExistsAsync("0912345678", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenWorkspaceDuplicationThrows_ShouldStillSucceed()
    {
        var fixture = CreateFixture();

        fixture.Workspaces
            .Setup(x => x.GetSystemDefaultWorkspaceAsync(It.IsAny<WorkspaceType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkspaceEntity { Id = 1, Type = WorkspaceType.Web, LayoutJson = "{}" });

        fixture.WorkspaceDuplicationService
            .Setup(x => x.DuplicateWorkspaceAsync(It.IsAny<WorkspaceEntity>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("dup failed"));

        var result = await fixture.Handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        fixture.Email.Verify(x => x.SendVerificationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenBothSystemDefaultsMissing_ShouldNotDuplicateAnyWorkspace()
    {
        var fixture = CreateFixture();
        fixture.Workspaces
            .Setup(x => x.GetSystemDefaultWorkspaceAsync(It.IsAny<WorkspaceType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkspaceEntity?)null);

        var result = await fixture.Handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        fixture.WorkspaceDuplicationService.Verify(
            x => x.DuplicateWorkspaceAsync(It.IsAny<WorkspaceEntity>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldHashPasswordWhenCreatingUser()
    {
        var fixture = CreateFixture();
        User? savedUser = null;

        fixture.Users
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => savedUser = user)
            .Returns(Task.CompletedTask);

        fixture.Workspaces
            .Setup(x => x.GetSystemDefaultWorkspaceAsync(It.IsAny<WorkspaceType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkspaceEntity?)null);

        await fixture.Handler.Handle(CreateCommand(password: "Password1"), CancellationToken.None);

        Assert.NotNull(savedUser);
        Assert.NotEqual("Password1", savedUser!.HashedPassword);
        Assert.True(BCrypt.Net.BCrypt.Verify("Password1", savedUser.HashedPassword));
    }

    private static RegisterCommand CreateCommand(
        string? email = null,
        string? password = null,
        string? fullName = null,
        string? phoneNumber = null)
    {
        return new RegisterCommand(
            email ?? "new@example.com",
            password ?? "Password1",
            fullName ?? "New User",
            phoneNumber ?? "0912345678");
    }

    private static RegisterFixture CreateFixture(Dictionary<string, string?>? configurationValues = null)
    {
        var users = new Mock<IUserRepository>();
        var workspaces = new Mock<IWorkspaceRepository>();
        var uow = new Mock<IUnitOfWork>();
        var email = new Mock<IEmailService>();
        var redis = new Mock<IRedisService>();
        var workspaceDuplicationService = new Mock<IWorkspaceDuplicationService>();

        users.Setup(x => x.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        users.Setup(x => x.PhoneNumberExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        users.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        uow.SetupGet(x => x.Users).Returns(users.Object);
        uow.SetupGet(x => x.Workspaces).Returns(workspaces.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        redis.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<TimeSpan?>())).ReturnsAsync(true);
        email.Setup(x => x.SendVerificationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues ?? new Dictionary<string, string?>())
            .Build();

        var handler = new RegisterCommandHandler(
            uow.Object,
            email.Object,
            redis.Object,
            configuration,
            workspaceDuplicationService.Object,
            Mock.Of<ILogger<RegisterCommandHandler>>());

        return new RegisterFixture(handler, uow, users, workspaces, email, redis, workspaceDuplicationService);
    }

    private sealed record RegisterFixture(
        RegisterCommandHandler Handler,
        Mock<IUnitOfWork> Uow,
        Mock<IUserRepository> Users,
        Mock<IWorkspaceRepository> Workspaces,
        Mock<IEmailService> Email,
        Mock<IRedisService> Redis,
        Mock<IWorkspaceDuplicationService> WorkspaceDuplicationService);
}