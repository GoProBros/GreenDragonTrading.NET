using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Application.UseCases.Auth.Commands.Login;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace GreenDragonTrading.Application.Tests.UseCases.Auth.Commands.Login;

public class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        var fixture = CreateFixture();
        fixture.Users.Setup(x => x.GetByEmailAsync("missing@example.com", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => fixture.Handler.Handle(CreateCommand(email: "missing@example.com"), CancellationToken.None));

        Assert.Equal("Email không tồn tại.", exception.Message);
        fixture.Jwt.VerifyNoOtherCalls();
        fixture.Redis.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenUserInactive_ShouldThrowBusinessRuleException()
    {
        var fixture = CreateFixture();
        fixture.Users.Setup(x => x.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(CreateUser(status: CommonStatus.InActive));

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => fixture.Handler.Handle(CreateCommand(), CancellationToken.None));

        Assert.Equal("Tài khoản đã bị khoá.", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenPasswordInvalid_ShouldThrowUnauthenticatedException()
    {
        var fixture = CreateFixture();
        fixture.Users.Setup(x => x.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(CreateUser(hashedPassword: BCrypt.Net.BCrypt.HashPassword("Password1")));

        var exception = await Assert.ThrowsAsync<UnauthenticatedException>(() => fixture.Handler.Handle(CreateCommand(password: "WrongPass1"), CancellationToken.None));

        Assert.Equal("Mật khẩu không chính xác.", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenEmailNotVerified_ShouldThrowBusinessRuleException()
    {
        var fixture = CreateFixture();
        fixture.Users.Setup(x => x.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(CreateUser(isVerified: false));

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => fixture.Handler.Handle(CreateCommand(), CancellationToken.None));

        Assert.Equal("Email chưa được xác thực", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenUserHasActiveSubscription_ShouldReturnJwtTokensAndPremiumLevel()
    {
        var fixture = CreateFixture();
        var user = CreateUser();
        var subscription = new UserSubscription
        {
            UserId = user.Id,
            Subscription = new Subscription
            {
                Id = 7,
                Name = "Vip 2",
                LevelOrder = SubscriptionLevel.VipTwo,
                MaxWorkspaces = 5,
                Price = 500000,
                DurationInDays = 30
            }
        };

        fixture.Users.Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        fixture.UserSubscriptions.Setup(x => x.GetActiveSubscriptionAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(subscription);
        fixture.Jwt.Setup(x => x.GenerateAccessToken(user.Id, user.Email, user.Username, user.PhoneNumber, user.Role.ToString(), SubscriptionLevel.VipTwo.GetDisplayName())).Returns("access-token");
        fixture.Jwt.Setup(x => x.GenerateRefreshToken()).Returns("refresh-token");

        var result = await fixture.Handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Đăng nhập thành công.", result.Message);
        Assert.Equal("access-token", result.Data!.AccessToken);
        Assert.Equal("refresh-token", result.Data.RefreshToken);
        Assert.Equal("Vip 2", result.Data.User.SubscriptionLevel);
        fixture.Redis.Verify(x => x.SetAsync("refresh:token:refresh-token", user.Id, TimeSpan.FromDays(7)), Times.Once);
        fixture.Jwt.Verify(x => x.GenerateAccessToken(user.Id, user.Email, user.Username, user.PhoneNumber, user.Role.ToString(), "Vip 2"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoSubscription_ShouldUseFreeLevel()
    {
        var fixture = CreateFixture();
        var user = CreateUser();

        fixture.Users.Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        fixture.UserSubscriptions.Setup(x => x.GetActiveSubscriptionAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync((UserSubscription?)null);
        fixture.Jwt.Setup(x => x.GenerateAccessToken(user.Id, user.Email, user.Username, user.PhoneNumber, user.Role.ToString(), SubscriptionLevel.Free.GetDisplayName())).Returns("access-token");
        fixture.Jwt.Setup(x => x.GenerateRefreshToken()).Returns("refresh-token");

        var result = await fixture.Handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.Equal("Free", result.Data!.User.SubscriptionLevel);
        fixture.Jwt.Verify(x => x.GenerateAccessToken(user.Id, user.Email, user.Username, user.PhoneNumber, user.Role.ToString(), "Free"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserHasTelegramId_ShouldSetTelegramFlagsInResponse()
    {
        var fixture = CreateFixture();
        var user = CreateUser();
        user.TelegramId = "123456789";

        fixture.Users.Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        fixture.UserSubscriptions.Setup(x => x.GetActiveSubscriptionAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync((UserSubscription?)null);

        var result = await fixture.Handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.True(result.Data!.User.IsTelegramLinked);
        Assert.Equal("123456789", result.Data.User.TelegramChatId);
    }

    [Fact]
    public async Task Handle_WhenLoginSuccess_ShouldSetRedisRefreshTokenWithConfiguredTtl()
    {
        var fixture = CreateFixture();
        var user = CreateUser();

        fixture.Users.Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        fixture.UserSubscriptions.Setup(x => x.GetActiveSubscriptionAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync((UserSubscription?)null);

        await fixture.Handler.Handle(CreateCommand(), CancellationToken.None);

        fixture.Redis.Verify(x => x.SetAsync(It.Is<string>(k => k.StartsWith("refresh:token:")), user.Id, TimeSpan.FromDays(7)), Times.Once);
    }

    private static LoginCommand CreateCommand(string? email = null, string? password = null)
    {
        return new LoginCommand(email ?? "user@example.com", password ?? "Password1");
    }

    private static User CreateUser(
        CommonStatus status = CommonStatus.Active,
        bool isVerified = true,
        string? hashedPassword = null)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            Username = "Test User",
            PhoneNumber = "0912345678",
            HashedPassword = hashedPassword ?? BCrypt.Net.BCrypt.HashPassword("Password1"),
            Role = UserRole.User,
            Status = status,
            IsEmailVerified = isVerified
        };
    }

    private static LoginFixture CreateFixture()
    {
        var users = new Mock<IUserRepository>();
        var userSubscriptions = new Mock<IUserSubscriptionRepository>();
        var uow = new Mock<IUnitOfWork>();
        var jwt = new Mock<IJwtService>();
        var redis = new Mock<IRedisService>();

        users.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        userSubscriptions.Setup(x => x.GetActiveSubscriptionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((UserSubscription?)null);
        redis.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<TimeSpan?>())).ReturnsAsync(true);
        jwt.Setup(x => x.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>())).Returns("access-token");
        jwt.Setup(x => x.GenerateRefreshToken()).Returns("refresh-token");

        uow.SetupGet(x => x.Users).Returns(users.Object);
        uow.SetupGet(x => x.UserSubscriptions).Returns(userSubscriptions.Object);

        var handler = new LoginCommandHandler(
            uow.Object,
            jwt.Object,
            redis.Object,
            Mock.Of<ILogger<LoginCommandHandler>>(),
            Options.Create(new JwtOptions
            {
                AccessTokenExpirationMinutes = 15,
                RefreshTokenExpirationDays = 7,
                Secret = "secret",
                Issuer = "issuer",
                Audience = "audience"
            }));

        return new LoginFixture(handler, users, userSubscriptions, jwt, redis);
    }

    private sealed record LoginFixture(
        LoginCommandHandler Handler,
        Mock<IUserRepository> Users,
        Mock<IUserSubscriptionRepository> UserSubscriptions,
        Mock<IJwtService> Jwt,
        Mock<IRedisService> Redis);
}