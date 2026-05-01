using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Application.UseCases.Alerts.Commands.CreateAlert;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GreenDragonTrading.Application.Tests.UseCases.Alerts.Commands.CreateAlert;

public class CreateAlertCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenSymbolDoesNotExist_ShouldThrowNotFoundException()
    {
        var fixture = CreateFixture();
        fixture.Symbols.Setup(x => x.GetByIdAsync("FPT", It.IsAny<CancellationToken>())).ReturnsAsync((Symbol?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => fixture.Handler.Handle(CreatePriceAlertCommand(), CancellationToken.None));

        Assert.Equal("Mã cổ phiếu không tồn tại", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenPriceAboveThreshold_ShouldPersistAlertAndAddRedisEntry()
    {
        var fixture = CreateFixture();
        Alert? savedAlert = null;

        fixture.Symbols.Setup(x => x.GetByIdAsync("FPT", It.IsAny<CancellationToken>())).ReturnsAsync(new Symbol { Ticker = "FPT" });
        fixture.Redis.Setup(x => x.GetHashAsync<MarketSymbolDto>(RedisConstants.MarketDataSymbol("FPT"))).ReturnsAsync(new MarketSymbolDto { Ticker = "FPT", LastPrice = 102.5 });
        fixture.Alerts.Setup(x => x.AddAsync(It.IsAny<Alert>(), It.IsAny<CancellationToken>()))
            .Callback<Alert, CancellationToken>((alert, _) => savedAlert = alert)
            .Returns(Task.CompletedTask);

        var result = await fixture.Handler.Handle(new CreateAlertCommand
        {
            Ticker = "fpt",
            Type = AlertType.Price,
            Condition = ConditionType.Above,
            ThresholdValue = 105m,
            Name = "  Breakout  ",
            IsActive = true,
            ChatSessionId = 9,
            MessageTemplate = "  Hold tight  "
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(savedAlert);
        Assert.Equal("FPT", savedAlert!.Ticker);
        Assert.Equal(105m, savedAlert.ThresholdValue);
        Assert.Null(savedAlert.ChangePercentage);
        Assert.Equal("Breakout", savedAlert.Name);
        Assert.Equal("Hold tight", savedAlert.MessageTemplate);
        Assert.Equal(NotificationChannel.System, savedAlert.NotifyVia);
        fixture.Redis.Verify(x => x.SortedSetAddAsync(RedisConstants.AlertsByTypeAndCondition("FPT", AlertType.Price, ConditionType.Above), It.IsAny<string>(), 105d), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPriceBelowThreshold_ShouldUseBelowRedisKey()
    {
        var fixture = CreateFixture();
        fixture.Symbols.Setup(x => x.GetByIdAsync("FPT", It.IsAny<CancellationToken>())).ReturnsAsync(new Symbol { Ticker = "FPT" });
        fixture.Redis.Setup(x => x.GetHashAsync<MarketSymbolDto>(RedisConstants.MarketDataSymbol("FPT"))).ReturnsAsync(new MarketSymbolDto { Ticker = "FPT", LastPrice = 100.0 });
        fixture.Alerts.Setup(x => x.AddAsync(It.IsAny<Alert>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await fixture.Handler.Handle(new CreateAlertCommand
        {
            Ticker = "FPT",
            Type = AlertType.Price,
            Condition = ConditionType.Below,
            ThresholdValue = 95m,
            IsActive = true
        }, CancellationToken.None);

        fixture.Redis.Verify(x => x.SortedSetAddAsync(RedisConstants.AlertsByTypeAndCondition("FPT", AlertType.Price, ConditionType.Below), It.IsAny<string>(), 95d), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenVolumePercentChangeUp_ShouldCalculateThresholdFromCurrentVolume()
    {
        var fixture = CreateFixture();
        Alert? savedAlert = null;

        fixture.Symbols.Setup(x => x.GetByIdAsync("FPT", It.IsAny<CancellationToken>())).ReturnsAsync(new Symbol { Ticker = "FPT" });
        fixture.Redis.Setup(x => x.GetHashAsync<MarketSymbolDto>(RedisConstants.MarketDataSymbol("FPT"))).ReturnsAsync(new MarketSymbolDto { Ticker = "FPT", TotalVol = 1000 });
        fixture.Alerts.Setup(x => x.AddAsync(It.IsAny<Alert>(), It.IsAny<CancellationToken>()))
            .Callback<Alert, CancellationToken>((alert, _) => savedAlert = alert)
            .Returns(Task.CompletedTask);

        await fixture.Handler.Handle(new CreateAlertCommand
        {
            Ticker = "fpt",
            Type = AlertType.Volume,
            Condition = ConditionType.PercentChangeUp,
            ChangePercentage = 10m,
            IsActive = true
        }, CancellationToken.None);

        Assert.NotNull(savedAlert);
        Assert.Equal(1100m, savedAlert!.ThresholdValue);
        Assert.Equal(10m, savedAlert.ChangePercentage);
    }

    [Fact]
    public async Task Handle_WhenCurrentPriceIsZero_ShouldThrowBusinessRuleException()
    {
        var fixture = CreateFixture();
        fixture.Symbols.Setup(x => x.GetByIdAsync("FPT", It.IsAny<CancellationToken>())).ReturnsAsync(new Symbol { Ticker = "FPT" });
        fixture.Redis.Setup(x => x.GetHashAsync<MarketSymbolDto>(RedisConstants.MarketDataSymbol("FPT"))).ReturnsAsync(new MarketSymbolDto { Ticker = "FPT", LastPrice = 0 });

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => fixture.Handler.Handle(CreatePriceAlertCommand(), CancellationToken.None));

        Assert.Equal("Giá hiện tại chưa sẵn sàng để đặt cảnh báo", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenThresholdMissingForAbove_ShouldThrowBusinessRuleException()
    {
        var fixture = CreateFixture();
        fixture.Symbols.Setup(x => x.GetByIdAsync("FPT", It.IsAny<CancellationToken>())).ReturnsAsync(new Symbol { Ticker = "FPT" });
        fixture.Redis.Setup(x => x.GetHashAsync<MarketSymbolDto>(RedisConstants.MarketDataSymbol("FPT"))).ReturnsAsync(new MarketSymbolDto { Ticker = "FPT", LastPrice = 101 });

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => fixture.Handler.Handle(new CreateAlertCommand
        {
            Ticker = "FPT",
            Type = AlertType.Price,
            Condition = ConditionType.Above,
            IsActive = true
        }, CancellationToken.None));

        Assert.Equal("Condition 1,2 yêu cầu thresholdValue", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenMarketDataMissing_ShouldThrowBusinessRuleException()
    {
        var fixture = CreateFixture();
        fixture.Symbols.Setup(x => x.GetByIdAsync("FPT", It.IsAny<CancellationToken>())).ReturnsAsync(new Symbol { Ticker = "FPT" });
        fixture.Redis.Setup(x => x.GetHashAsync<MarketSymbolDto>(RedisConstants.MarketDataSymbol("FPT"))).ReturnsAsync((MarketSymbolDto?)null);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => fixture.Handler.Handle(CreatePriceAlertCommand(), CancellationToken.None));

        Assert.Equal("Không tìm thấy dữ liệu thị trường hiện tại trên Redis", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenPercentConditionMissingChangePercentage_ShouldThrowBusinessRuleException()
    {
        var fixture = CreateFixture();
        fixture.Symbols.Setup(x => x.GetByIdAsync("FPT", It.IsAny<CancellationToken>())).ReturnsAsync(new Symbol { Ticker = "FPT" });
        fixture.Redis.Setup(x => x.GetHashAsync<MarketSymbolDto>(RedisConstants.MarketDataSymbol("FPT"))).ReturnsAsync(new MarketSymbolDto { Ticker = "FPT", LastPrice = 100 });

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => fixture.Handler.Handle(new CreateAlertCommand
        {
            Ticker = "FPT",
            Type = AlertType.Price,
            Condition = ConditionType.PercentChangeUp,
            IsActive = true
        }, CancellationToken.None));

        Assert.Equal("Condition 3,4 yêu cầu changePercentage", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenPercentChangeDown_ShouldCalculateThresholdFromCurrentPrice()
    {
        var fixture = CreateFixture();
        Alert? savedAlert = null;
        fixture.Symbols.Setup(x => x.GetByIdAsync("FPT", It.IsAny<CancellationToken>())).ReturnsAsync(new Symbol { Ticker = "FPT" });
        fixture.Redis.Setup(x => x.GetHashAsync<MarketSymbolDto>(RedisConstants.MarketDataSymbol("FPT"))).ReturnsAsync(new MarketSymbolDto { Ticker = "FPT", LastPrice = 100 });
        fixture.Alerts.Setup(x => x.AddAsync(It.IsAny<Alert>(), It.IsAny<CancellationToken>()))
            .Callback<Alert, CancellationToken>((alert, _) => savedAlert = alert)
            .Returns(Task.CompletedTask);

        await fixture.Handler.Handle(new CreateAlertCommand
        {
            Ticker = "FPT",
            Type = AlertType.Price,
            Condition = ConditionType.PercentChangeDown,
            ChangePercentage = 5m,
            IsActive = true
        }, CancellationToken.None);

        Assert.NotNull(savedAlert);
        Assert.Equal(95m, savedAlert!.ThresholdValue);
    }

    [Fact]
    public async Task Handle_WhenAlertIsInactive_ShouldNotAddToRedisSortedSet()
    {
        var fixture = CreateFixture();
        fixture.Symbols.Setup(x => x.GetByIdAsync("FPT", It.IsAny<CancellationToken>())).ReturnsAsync(new Symbol { Ticker = "FPT" });
        fixture.Redis.Setup(x => x.GetHashAsync<MarketSymbolDto>(RedisConstants.MarketDataSymbol("FPT"))).ReturnsAsync(new MarketSymbolDto { Ticker = "FPT", LastPrice = 101 });
        fixture.Alerts.Setup(x => x.AddAsync(It.IsAny<Alert>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await fixture.Handler.Handle(new CreateAlertCommand
        {
            Ticker = "FPT",
            Type = AlertType.Price,
            Condition = ConditionType.Above,
            ThresholdValue = 110m,
            IsActive = false
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        fixture.Redis.Verify(x => x.SortedSetAddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double>()), Times.Never);
    }

    private static CreateAlertCommand CreatePriceAlertCommand()
    {
        return new CreateAlertCommand
        {
            Ticker = "FPT",
            Type = AlertType.Price,
            Condition = ConditionType.Above,
            ThresholdValue = 105m,
            IsActive = true
        };
    }

    private static AlertFixture CreateFixture()
    {
        var uow = new Mock<IUnitOfWork>();
        var currentUser = new Mock<ICurrentUserService>();
        var redis = new Mock<IRedisService>();
        var symbols = new Mock<ISymbolRepository>();
        var alerts = new Mock<IAlertRepository>();
        var userId = Guid.NewGuid();

        currentUser.Setup(x => x.GetRequiredUserId()).Returns(userId);
        symbols.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Symbol?)null);
        alerts.Setup(x => x.AddAsync(It.IsAny<Alert>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        uow.SetupGet(x => x.Symbols).Returns(symbols.Object);
        uow.SetupGet(x => x.Alerts).Returns(alerts.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        redis.Setup(x => x.GetHashAsync<MarketSymbolDto>(It.IsAny<string>())).ReturnsAsync((MarketSymbolDto?)null);
        redis.Setup(x => x.SortedSetAddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double>())).ReturnsAsync(true);

        var handler = new CreateAlertCommandHandler(
            uow.Object,
            currentUser.Object,
            redis.Object);

        return new AlertFixture(handler, uow, currentUser, redis, symbols, alerts, userId);
    }

    private sealed record AlertFixture(
        CreateAlertCommandHandler Handler,
        Mock<IUnitOfWork> Uow,
        Mock<ICurrentUserService> CurrentUser,
        Mock<IRedisService> Redis,
        Mock<ISymbolRepository> Symbols,
        Mock<IAlertRepository> Alerts,
        Guid UserId);
}