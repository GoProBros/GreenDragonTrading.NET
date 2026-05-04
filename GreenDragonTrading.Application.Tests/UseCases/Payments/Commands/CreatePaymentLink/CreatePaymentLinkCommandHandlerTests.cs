using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Application.UseCases.Payments.Commands.CreatePaymentLink;
using GreenDragonTrading.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GreenDragonTrading.Application.Tests.UseCases.Payments.Commands.CreatePaymentLink;

public class CreatePaymentLinkCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenProviderIsPayos_ShouldCallPayosServiceAndReturnResult()
    {
        var fixture = CreateFixture();
        var expected = new PaymentLinkResponse { CheckoutUrl = "https://payos.example.com/checkout", OrderCode = 123456 };

        fixture.PaymentService
            .Setup(x => x.CreateVipPaymentAsync(fixture.UserId, 9, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await fixture.Handler.Handle(new CreatePaymentLinkCommand(9, PaymentType.Payos), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected.CheckoutUrl, result.Data!.CheckoutUrl);
        Assert.Equal(expected.OrderCode, result.Data.OrderCode);
        fixture.PaymentService.Verify(x => x.CreateVipPaymentAsync(fixture.UserId, 9, It.IsAny<CancellationToken>()), Times.Once);
        fixture.PaymentService.Verify(x => x.CreateMomoVipPaymentAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenProviderIsMomo_ShouldCallMomoServiceAndMapOrderCode()
    {
        var fixture = CreateFixture();
        var momo = new MomoPaymentLinkResponse { PayUrl = "https://momo.example.com/pay", OrderId = "987654" };

        fixture.PaymentService
            .Setup(x => x.CreateMomoVipPaymentAsync(fixture.UserId, 11, It.IsAny<CancellationToken>()))
            .ReturnsAsync(momo);

        var result = await fixture.Handler.Handle(new CreatePaymentLinkCommand(11, PaymentType.Momo), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(momo.PayUrl, result.Data!.CheckoutUrl);
        Assert.Equal(987654, result.Data.OrderCode);
        fixture.PaymentService.Verify(x => x.CreateMomoVipPaymentAsync(fixture.UserId, 11, It.IsAny<CancellationToken>()), Times.Once);
        fixture.PaymentService.Verify(x => x.CreateVipPaymentAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenProviderIsMomo_ShouldReturnSuccessMessage()
    {
        var fixture = CreateFixture();
        fixture.PaymentService
            .Setup(x => x.CreateMomoVipPaymentAsync(fixture.UserId, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MomoPaymentLinkResponse { PayUrl = "https://momo.example.com/pay", OrderId = "123" });

        var result = await fixture.Handler.Handle(new CreatePaymentLinkCommand(3, PaymentType.Momo), CancellationToken.None);

        Assert.Equal("Tạo đường dẫn thanh toán thành công.", result.Message);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenProviderIsPayos_ShouldUseCurrentUserIdAndSubscriptionId()
    {
        var fixture = CreateFixture();
        fixture.PaymentService
            .Setup(x => x.CreateVipPaymentAsync(fixture.UserId, 77, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentLinkResponse { CheckoutUrl = "https://payos.example.com/checkout/77", OrderCode = 777 });

        var result = await fixture.Handler.Handle(new CreatePaymentLinkCommand(77, PaymentType.Payos), CancellationToken.None);

        Assert.Equal(777, result.Data!.OrderCode);
        fixture.CurrentUser.Verify(x => x.GetRequiredUserId(), Times.Once);
        fixture.PaymentService.Verify(x => x.CreateVipPaymentAsync(fixture.UserId, 77, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenProviderIsMomo_ShouldUseCurrentUserIdAndSubscriptionId()
    {
        var fixture = CreateFixture();
        fixture.PaymentService
            .Setup(x => x.CreateMomoVipPaymentAsync(fixture.UserId, 88, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MomoPaymentLinkResponse { PayUrl = "https://momo.example.com/pay/88", OrderId = "888" });

        var result = await fixture.Handler.Handle(new CreatePaymentLinkCommand(88, PaymentType.Momo), CancellationToken.None);

        Assert.Equal(888, result.Data!.OrderCode);
        fixture.CurrentUser.Verify(x => x.GetRequiredUserId(), Times.Once);
        fixture.PaymentService.Verify(x => x.CreateMomoVipPaymentAsync(fixture.UserId, 88, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenProviderIsPayos_ShouldReturnSuccessMessage()
    {
        var fixture = CreateFixture();
        fixture.PaymentService
            .Setup(x => x.CreateVipPaymentAsync(fixture.UserId, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentLinkResponse { CheckoutUrl = "https://payos.example.com/checkout/5", OrderCode = 5005 });

        var result = await fixture.Handler.Handle(new CreatePaymentLinkCommand(5, PaymentType.Payos), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Tạo đường dẫn thanh toán thành công.", result.Message);
    }

    [Fact]
    public async Task Handle_WhenMomoOrderIdIsNotNumeric_ShouldThrowFormatException()
    {
        var fixture = CreateFixture();
        fixture.PaymentService
            .Setup(x => x.CreateMomoVipPaymentAsync(fixture.UserId, 4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MomoPaymentLinkResponse { PayUrl = "https://momo.example.com", OrderId = "not-a-number" });

        await Assert.ThrowsAsync<FormatException>(() => fixture.Handler.Handle(new CreatePaymentLinkCommand(4, PaymentType.Momo), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenPayosServiceThrows_ShouldPropagateException()
    {
        var fixture = CreateFixture();
        fixture.PaymentService
            .Setup(x => x.CreateVipPaymentAsync(fixture.UserId, 6, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("payos failed"));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Handler.Handle(new CreatePaymentLinkCommand(6, PaymentType.Payos), CancellationToken.None));

        Assert.Equal("payos failed", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenMomoServiceThrows_ShouldPropagateException()
    {
        var fixture = CreateFixture();
        fixture.PaymentService
            .Setup(x => x.CreateMomoVipPaymentAsync(fixture.UserId, 7, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("momo failed"));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Handler.Handle(new CreatePaymentLinkCommand(7, PaymentType.Momo), CancellationToken.None));

        Assert.Equal("momo failed", exception.Message);
    }

    private static PaymentFixture CreateFixture()
    {
        var paymentService = new Mock<IPaymentService>();
        var currentUser = new Mock<ICurrentUserService>();
        var userId = Guid.NewGuid();

        currentUser.Setup(x => x.GetRequiredUserId()).Returns(userId);
        paymentService.Setup(x => x.CreateVipPaymentAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentLinkResponse());
        paymentService.Setup(x => x.CreateMomoVipPaymentAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MomoPaymentLinkResponse());

        var handler = new CreatePaymentLinkCommandHandler(
            paymentService.Object,
            currentUser.Object,
            Mock.Of<ILogger<CreatePaymentLinkCommandHandler>>());

        return new PaymentFixture(handler, paymentService, currentUser, userId);
    }

    private sealed record PaymentFixture(
        CreatePaymentLinkCommandHandler Handler,
        Mock<IPaymentService> PaymentService,
        Mock<ICurrentUserService> CurrentUser,
        Guid UserId);
}