using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.DTOs.Realtime;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Application.UseCases.Alerts.Events;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using Moq;
using Xunit;

namespace GreenDragonTrading.Application.Tests.UseCases.Alerts.Events;

public class PriceUpdatedEventHandlerVolumeTests
{
    [Fact]
    public async Task Handle_WhenVolumeAbove_ShouldTriggerUsingCurrentCandle()
    {
        var fixture = CreateFixture();
        var alert = BuildAlert(1, ConditionType.Above, thresholdValue: 150m);

        fixture.ConfigureAlerts(alert, ConditionType.Above);
        fixture.SetupCurrentCandleVolume(VolumeTimeFrame.M1, 200);
        fixture.SetupClosedCandles(VolumeTimeFrame.M1, new[]
        {
            (DateTime.UtcNow.AddMinutes(-1), 100L)
        });

        await fixture.Handler.Handle(new PriceUpdatedEvent("FPT", 100m), CancellationToken.None);

        Assert.True(alert.IsTriggered);
        fixture.Redis.Verify(x => x.SortedSetRemoveAsync(RedisConstants.AlertsByTypeAndCondition("FPT", AlertType.Volume, ConditionType.Above), "1"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenVolumeBelow_ShouldTriggerUsingClosedCandle()
    {
        var fixture = CreateFixture();
        var alert = BuildAlert(2, ConditionType.Below, thresholdValue: 90m);

        fixture.ConfigureAlerts(alert, ConditionType.Below);
        fixture.SetupCurrentCandleVolume(VolumeTimeFrame.M1, 200);
        fixture.SetupClosedCandles(VolumeTimeFrame.M1, new[]
        {
            (DateTime.UtcNow.AddMinutes(-1), 80L)
        });

        await fixture.Handler.Handle(new PriceUpdatedEvent("FPT", 100m), CancellationToken.None);

        Assert.True(alert.IsTriggered);
        fixture.Redis.Verify(x => x.SortedSetRemoveAsync(RedisConstants.AlertsByTypeAndCondition("FPT", AlertType.Volume, ConditionType.Below), "2"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenVolumePercentUp_ShouldTriggerAgainstMinLookback()
    {
        var fixture = CreateFixture();
        var alert = BuildPercentAlert(3, ConditionType.PercentChangeUp, 20m, lookbackBars: 3);

        fixture.ConfigureAlerts(alert, ConditionType.PercentChangeUp);
        fixture.SetupCurrentCandleVolume(VolumeTimeFrame.M1, 140);
        fixture.SetupClosedCandles(VolumeTimeFrame.M1, new[]
        {
            (DateTime.UtcNow.AddMinutes(-1), 110L),
            (DateTime.UtcNow.AddMinutes(-2), 120L),
            (DateTime.UtcNow.AddMinutes(-3), 100L)
        });

        await fixture.Handler.Handle(new PriceUpdatedEvent("FPT", 100m), CancellationToken.None);

        Assert.True(alert.IsTriggered);
        fixture.Redis.Verify(x => x.SortedSetRemoveAsync(RedisConstants.AlertsByTypeAndCondition("FPT", AlertType.Volume, ConditionType.PercentChangeUp), "3"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenVolumePercentDown_ShouldTriggerAgainstPreviousMin()
    {
        var fixture = CreateFixture();
        var alert = BuildPercentAlert(4, ConditionType.PercentChangeDown, 15m, lookbackBars: 3);

        fixture.ConfigureAlerts(alert, ConditionType.PercentChangeDown);
        fixture.SetupCurrentCandleVolume(VolumeTimeFrame.M1, 200);
        fixture.SetupClosedCandles(VolumeTimeFrame.M1, new[]
        {
            (DateTime.UtcNow.AddMinutes(-1), 80L),
            (DateTime.UtcNow.AddMinutes(-2), 100L),
            (DateTime.UtcNow.AddMinutes(-3), 120L),
            (DateTime.UtcNow.AddMinutes(-4), 130L)
        });

        await fixture.Handler.Handle(new PriceUpdatedEvent("FPT", 100m), CancellationToken.None);

        Assert.True(alert.IsTriggered);
        fixture.Redis.Verify(x => x.SortedSetRemoveAsync(RedisConstants.AlertsByTypeAndCondition("FPT", AlertType.Volume, ConditionType.PercentChangeDown), "4"), Times.Once);
    }

    private static Alert BuildAlert(int id, ConditionType condition, decimal thresholdValue)
    {
        return new Alert
        {
            Id = id,
            UserId = Guid.NewGuid(),
            Ticker = "FPT",
            Type = AlertType.Volume,
            Condition = condition,
            ThresholdValue = thresholdValue,
            VolumeTimeFrame = VolumeTimeFrame.M1,
            IsActive = true,
            IsTriggered = false
        };
    }

    private static Alert BuildPercentAlert(int id, ConditionType condition, decimal changePercentage, int lookbackBars)
    {
        return new Alert
        {
            Id = id,
            UserId = Guid.NewGuid(),
            Ticker = "FPT",
            Type = AlertType.Volume,
            Condition = condition,
            ChangePercentage = changePercentage,
            VolumeTimeFrame = VolumeTimeFrame.M1,
            VolumeLookbackBars = lookbackBars,
            IsActive = true,
            IsTriggered = false
        };
    }

    private static VolumeFixture CreateFixture()
    {
        var redis = new Mock<IRedisService>();
        var uow = new Mock<IUnitOfWork>();
        var ohlcvUow = new Mock<IOhlcvUnitOfWork>();
        var alerts = new Mock<IAlertRepository>();
        var alertTemplates = new Mock<IAlertTemplateRepository>();
        var chatSessions = new Mock<IChatSessionRepository>();
        var chatMessages = new Mock<IChatMessageRepository>();
        var chatParticipants = new Mock<IChatParticipantRepository>();
        var users = new Mock<IUserRepository>();
        var broadcaster = new Mock<INotificationBroadcaster>();
        var telegram = new Mock<ITelegramBotService>();

        redis.Setup(x => x.SortedSetRangeByScoreAsync(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(new List<string>());
        redis.Setup(x => x.SortedSetRemoveAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        redis.Setup(x => x.GetAsync<CurrentCandleDto>(It.IsAny<string>()))
            .ReturnsAsync((CurrentCandleDto?)null);

        alertTemplates.Setup(x => x.GetActiveByTypeAndConditionAsync(It.IsAny<AlertType>(), It.IsAny<ConditionType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AlertTemplate?)null);
        alertTemplates.Setup(x => x.GetDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((AlertTemplate?)null);

        chatSessions.Setup(x => x.GetSystemSessionByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatSession { Id = 1, SessionType = ChatSessionType.System, Status = CommonStatus.Active });
        chatParticipants.Setup(x => x.IsUserParticipantAsync(1, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        chatMessages.Setup(x => x.AddAsync(It.IsAny<ChatMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        users.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        broadcaster.Setup(x => x.BroadcastSystemChatMessageAsync(It.IsAny<Guid>(), It.IsAny<SystemChatMessageSignalREventDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        telegram.Setup(x => x.SendTextMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        uow.SetupGet(x => x.Alerts).Returns(alerts.Object);
        uow.SetupGet(x => x.AlertTemplates).Returns(alertTemplates.Object);
        uow.SetupGet(x => x.ChatSessions).Returns(chatSessions.Object);
        uow.SetupGet(x => x.ChatMessages).Returns(chatMessages.Object);
        uow.SetupGet(x => x.ChatParticipants).Returns(chatParticipants.Object);
        uow.SetupGet(x => x.Users).Returns(users.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        ohlcvUow.SetupGet(x => x.Ohlcv).Returns(Mock.Of<IOhlcvRepository>());

        var handler = new PriceUpdatedEventHandler(
            redis.Object,
            ohlcvUow.Object,
            uow.Object,
            broadcaster.Object,
            telegram.Object);

        return new VolumeFixture(handler, redis, alerts, ohlcvUow);
    }

    private sealed record VolumeFixture(
        PriceUpdatedEventHandler Handler,
        Mock<IRedisService> Redis,
        Mock<IAlertRepository> Alerts,
        Mock<IOhlcvUnitOfWork> OhlcvUow)
    {
        public void ConfigureAlerts(Alert alert, ConditionType condition)
        {
            var key = RedisConstants.AlertsByTypeAndCondition("FPT", AlertType.Volume, condition);
            Redis.Setup(x => x.SortedSetRangeByScoreAsync(key, It.IsAny<double>(), It.IsAny<double>()))
                .ReturnsAsync(new List<string> { alert.Id.ToString() });

            Alerts.Setup(x => x.GetActiveAlertsByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Alert> { alert });
        }

        public void SetupCurrentCandleVolume(VolumeTimeFrame timeframe, long volume)
        {
            var redisKey = RedisConstants.Ohlcv("FPT", timeframe.ToString());
            Redis.Setup(x => x.GetAsync<CurrentCandleDto>(redisKey))
                .ReturnsAsync(new CurrentCandleDto
                {
                    Ticker = "FPT",
                    Timeframe = timeframe.ToString(),
                    StartTime = DateTime.UtcNow.AddMinutes(-1),
                    LastUpdateTime = DateTime.UtcNow,
                    Open = 100,
                    High = 100,
                    Low = 100,
                    Close = 100,
                    Volume = volume,
                    TotalValue = 0,
                    IsComplete = false
                });
        }

        public void SetupClosedCandles(VolumeTimeFrame timeframe, IEnumerable<(DateTime Time, long Volume)> candles)
        {
            var ohlcvRepo = new Mock<IOhlcvRepository>();
            var list = candles.Select(c => new Domain.Entities.Ohlcv
            {
                Time = c.Time,
                Ticker = "FPT",
                Timeframe = timeframe.ToString(),
                Open = 100,
                High = 100,
                Low = 100,
                Close = 100,
                Volume = c.Volume,
                CreatedAt = c.Time
            }).ToList();

            ohlcvRepo.Setup(x => x.GetLatestCandlesAsync("FPT", timeframe.ToString(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            OhlcvUow.SetupGet(x => x.Ohlcv).Returns(ohlcvRepo.Object);
        }
    }
}