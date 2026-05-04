using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Application.UseCases.Auth.Commands.GoogleLogin;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using WorkspaceEntity = GreenDragonTrading.Domain.Entities.Workspace;

namespace GreenDragonTrading.Application.Tests.UseCases.Auth.Commands.GoogleLogin;

public class GoogleLoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenGoogleTokenInvalid_ShouldThrowUnauthenticatedException()
    {
        var fixture = CreateFixture();
        fixture.GoogleAuth.Setup(x => x.VerifyIdTokenAsync("bad-token", It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("invalid"));

        var exception = await Assert.ThrowsAsync<UnauthenticatedException>(() => fixture.Handler.Handle(new GoogleLoginCommand("bad-token"), CancellationToken.None));

        Assert.Equal("Google token không hợp lệ.", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenNewGoogleUser_ShouldCreateUserAndDuplicateBothDefaultWorkspaces()
    {
        var fixture = CreateFixture();
        var googleUser = new GoogleUserInfo
        {
            Email = "google@example.com",
            Name = "This name is longer than twenty four characters",
            PictureUrl = "https://img.example.com/avatar.png",
            GoogleId = "gid-1"
        };
        var duplicatedTypes = new List<WorkspaceType>();
        User? savedUser = null;

        fixture.GoogleAuth.Setup(x => x.VerifyIdTokenAsync("token-1", It.IsAny<CancellationToken>())).ReturnsAsync(googleUser);
        fixture.Users.Setup(x => x.GetByEmailAsync(googleUser.Email, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        fixture.Users.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => savedUser = user)
            .Returns(Task.CompletedTask);
        fixture.Workspaces.SetupSequence(x => x.GetSystemDefaultWorkspaceAsync(It.IsAny<WorkspaceType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkspaceEntity { Id = 1, Type = WorkspaceType.Web, LayoutJson = "{}" })
            .ReturnsAsync(new WorkspaceEntity { Id = 2, Type = WorkspaceType.Mobile, LayoutJson = "{}" });
        fixture.WorkspaceDuplicationService
            .Setup(x => x.DuplicateWorkspaceAsync(It.IsAny<WorkspaceEntity>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<WorkspaceEntity, Guid, string, CancellationToken>((workspace, _, _, _) => duplicatedTypes.Add(workspace.Type))
            .ReturnsAsync((WorkspaceEntity sourceWorkspace, Guid targetUserId, string suffix, CancellationToken _) => new WorkspaceEntity
            {
                Id = sourceWorkspace.Id + 10,
                UserId = targetUserId,
                WorkspaceName = sourceWorkspace.WorkspaceName + suffix,
                LayoutJson = sourceWorkspace.LayoutJson,
                Type = sourceWorkspace.Type,
                IsDefault = sourceWorkspace.IsDefault,
                ShareCode = "ABCDEFGH"
            });

        var result = await fixture.Handler.Handle(new GoogleLoginCommand("token-1"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(savedUser);
        Assert.Equal("google@example.com", savedUser!.Email);
        Assert.Equal(24, savedUser.Username.Length);
        Assert.Equal("This name is longer than", savedUser.Username);
        Assert.Equal("https://img.example.com/avatar.png", savedUser.AvatarUrl);
        Assert.True(savedUser.IsEmailVerified);
        Assert.Equal(CommonStatus.Active, savedUser.Status);
        Assert.Equal(2, duplicatedTypes.Count);
        Assert.Contains(WorkspaceType.Web, duplicatedTypes);
        Assert.Contains(WorkspaceType.Mobile, duplicatedTypes);
        fixture.Redis.Verify(x => x.SetAsync(It.IsAny<string>(), savedUser.Id, TimeSpan.FromDays(30)), Times.Once);
        fixture.Jwt.Verify(x => x.GenerateAccessToken(savedUser.Id, googleUser.Email, savedUser.Username, null, UserRole.User.ToString(), SubscriptionLevel.Free.GetDisplayName()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenExistingUserAvatarChanged_ShouldUpdateAvatar()
    {
        var fixture = CreateFixture();
        var user = CreateUser("old.png");
        var googleUser = new GoogleUserInfo
        {
            Email = user.Email,
            Name = user.Username,
            PictureUrl = "new.png",
            GoogleId = "gid-2"
        };

        fixture.GoogleAuth.Setup(x => x.VerifyIdTokenAsync("token-2", It.IsAny<CancellationToken>())).ReturnsAsync(googleUser);
        fixture.Users.Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await fixture.Handler.Handle(new GoogleLoginCommand("token-2"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("new.png", user.AvatarUrl);
        fixture.Uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenExistingUserAvatarSame_ShouldSkipUpdate()
    {
        var fixture = CreateFixture();
        var user = CreateUser("same.png");
        var googleUser = new GoogleUserInfo
        {
            Email = user.Email,
            Name = user.Username,
            PictureUrl = "same.png",
            GoogleId = "gid-3"
        };

        fixture.GoogleAuth.Setup(x => x.VerifyIdTokenAsync("token-3", It.IsAny<CancellationToken>())).ReturnsAsync(googleUser);
        fixture.Users.Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await fixture.Handler.Handle(new GoogleLoginCommand("token-3"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("same.png", user.AvatarUrl);
        fixture.Uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenExistingUserInactive_ShouldThrowBusinessRuleException()
    {
        var fixture = CreateFixture();
        var user = CreateUser("old.png", CommonStatus.InActive);
        var googleUser = new GoogleUserInfo
        {
            Email = user.Email,
            Name = user.Username,
            PictureUrl = "new.png",
            GoogleId = "gid-4"
        };

        fixture.GoogleAuth.Setup(x => x.VerifyIdTokenAsync("token-4", It.IsAny<CancellationToken>())).ReturnsAsync(googleUser);
        fixture.Users.Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => fixture.Handler.Handle(new GoogleLoginCommand("token-4"), CancellationToken.None));

        Assert.Equal("Tài khoản đã bị khoá.", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenNewUserAndSystemDefaultsMissing_ShouldStillSucceed()
    {
        var fixture = CreateFixture();
        var googleUser = new GoogleUserInfo
        {
            Email = "new-google@example.com",
            Name = "New Google User",
            PictureUrl = null,
            GoogleId = "gid-5"
        };

        fixture.GoogleAuth.Setup(x => x.VerifyIdTokenAsync("token-5", It.IsAny<CancellationToken>())).ReturnsAsync(googleUser);
        fixture.Users.Setup(x => x.GetByEmailAsync(googleUser.Email, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        fixture.Workspaces.Setup(x => x.GetSystemDefaultWorkspaceAsync(It.IsAny<WorkspaceType>(), It.IsAny<CancellationToken>())).ReturnsAsync((WorkspaceEntity?)null);

        var result = await fixture.Handler.Handle(new GoogleLoginCommand("token-5"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        fixture.WorkspaceDuplicationService.Verify(
            x => x.DuplicateWorkspaceAsync(It.IsAny<WorkspaceEntity>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenExistingUserWithSubscription_ShouldUseSubscriptionLevelInToken()
    {
        var fixture = CreateFixture();
        var user = CreateUser("avatar.png");
        var googleUser = new GoogleUserInfo
        {
            Email = user.Email,
            Name = user.Username,
            PictureUrl = user.AvatarUrl,
            GoogleId = "gid-6"
        };

        fixture.GoogleAuth.Setup(x => x.VerifyIdTokenAsync("token-6", It.IsAny<CancellationToken>())).ReturnsAsync(googleUser);
        fixture.Users.Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        fixture.UserSubscriptions.Setup(x => x.GetActiveSubscriptionAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new UserSubscription
        {
            UserId = user.Id,
            Subscription = new Subscription { Id = 10, Name = "Vip 1", LevelOrder = SubscriptionLevel.VipOne, MaxWorkspaces = 3, DurationInDays = 30, Price = 100000 }
        });

        await fixture.Handler.Handle(new GoogleLoginCommand("token-6"), CancellationToken.None);

        fixture.Jwt.Verify(x => x.GenerateAccessToken(user.Id, user.Email, user.Username, user.PhoneNumber, user.Role.ToString(), "Vip 1"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenLoginSuccess_ShouldStoreRefreshTokenWithConfiguredTtl()
    {
        var fixture = CreateFixture();
        var user = CreateUser("avatar.png");
        var googleUser = new GoogleUserInfo
        {
            Email = user.Email,
            Name = user.Username,
            PictureUrl = user.AvatarUrl,
            GoogleId = "gid-7"
        };

        fixture.GoogleAuth.Setup(x => x.VerifyIdTokenAsync("token-7", It.IsAny<CancellationToken>())).ReturnsAsync(googleUser);
        fixture.Users.Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        await fixture.Handler.Handle(new GoogleLoginCommand("token-7"), CancellationToken.None);

        fixture.Redis.Verify(x => x.SetAsync(It.Is<string>(k => k.StartsWith("refresh:token:")), user.Id, TimeSpan.FromDays(30)), Times.Once);
    }

    private static User CreateUser(string? avatarUrl = null, CommonStatus status = CommonStatus.Active)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "google@example.com",
            Username = "Google User",
            AvatarUrl = avatarUrl,
            PhoneNumber = null,
            HashedPassword = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
            Role = UserRole.User,
            IsEmailVerified = true,
            Status = status
        };
    }

    private static GoogleLoginFixture CreateFixture()
    {
        var users = new Mock<IUserRepository>();
        var userSubscriptions = new Mock<IUserSubscriptionRepository>();
        var workspaces = new Mock<IWorkspaceRepository>();
        var uow = new Mock<IUnitOfWork>();
        var jwt = new Mock<IJwtService>();
        var redis = new Mock<IRedisService>();
        var googleAuth = new Mock<IGoogleAuthService>();
        var workspaceDuplicationService = new Mock<IWorkspaceDuplicationService>();

        users.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        users.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        userSubscriptions.Setup(x => x.GetActiveSubscriptionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((UserSubscription?)null);
        redis.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<TimeSpan?>())).ReturnsAsync(true);
        jwt.Setup(x => x.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>())).Returns("access-token");
        jwt.Setup(x => x.GenerateRefreshToken()).Returns("refresh-token");
        googleAuth.Setup(x => x.VerifyIdTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new GoogleUserInfo());
        workspaces.Setup(x => x.GetSystemDefaultWorkspaceAsync(It.IsAny<WorkspaceType>(), It.IsAny<CancellationToken>())).ReturnsAsync((WorkspaceEntity?)null);

        uow.SetupGet(x => x.Users).Returns(users.Object);
        uow.SetupGet(x => x.UserSubscriptions).Returns(userSubscriptions.Object);
        uow.SetupGet(x => x.Workspaces).Returns(workspaces.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new GoogleLoginCommandHandler(
            uow.Object,
            jwt.Object,
            redis.Object,
            googleAuth.Object,
            workspaceDuplicationService.Object,
            Mock.Of<ILogger<GoogleLoginCommandHandler>>(),
            Options.Create(new JwtOptions
            {
                AccessTokenExpirationMinutes = 15,
                RefreshTokenExpirationDays = 30,
                Secret = "secret",
                Issuer = "issuer",
                Audience = "audience"
            }));

        return new GoogleLoginFixture(handler, users, userSubscriptions, workspaces, jwt, redis, googleAuth, workspaceDuplicationService, uow);
    }

    private sealed record GoogleLoginFixture(
        GoogleLoginCommandHandler Handler,
        Mock<IUserRepository> Users,
        Mock<IUserSubscriptionRepository> UserSubscriptions,
        Mock<IWorkspaceRepository> Workspaces,
        Mock<IJwtService> Jwt,
        Mock<IRedisService> Redis,
        Mock<IGoogleAuthService> GoogleAuth,
        Mock<IWorkspaceDuplicationService> WorkspaceDuplicationService,
        Mock<IUnitOfWork> Uow);
}