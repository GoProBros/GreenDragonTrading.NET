using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Payments.Commands.CreatePaymentLink
{
    /// <summary>
    /// Creates a payment link for the authenticated user.
    /// Routes to PayOS or Momo depending on <see cref="CreatePaymentLinkCommand.PaymentProvider"/>.
    /// </summary>
    public class CreatePaymentLinkCommandHandler : IRequestHandler<CreatePaymentLinkCommand, ApiResponse<PaymentLinkResponse>>
    {
        private readonly IPaymentService _paymentService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CreatePaymentLinkCommandHandler> _logger;

        public CreatePaymentLinkCommandHandler(
            IPaymentService paymentService,
            ICurrentUserService currentUserService,
            ILogger<CreatePaymentLinkCommandHandler> logger)
        {
            _paymentService = paymentService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ApiResponse<PaymentLinkResponse>> Handle(
            CreatePaymentLinkCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetRequiredUserId();

            _logger.LogInformation(
                "Creating payment link for UserId={UserId}, SubscriptionId={SubscriptionId}, Provider={Provider}",
                userId, request.SubscriptionId, request.PaymentProvider);

            PaymentLinkResponse result;

            if (request.PaymentProvider == PaymentType.Momo)
            {
                var momoResult = await _paymentService.CreateMomoVipPaymentAsync(
                    userId, request.SubscriptionId, cancellationToken);

                result = new PaymentLinkResponse
                {
                    CheckoutUrl = momoResult.PayUrl,
                    OrderCode = long.Parse(momoResult.OrderId)
                };
            }
            else
            {
                result = await _paymentService.CreateVipPaymentAsync(
                    userId, request.SubscriptionId, cancellationToken);
            }

            _logger.LogInformation(
                "Payment link created: OrderCode={OrderCode}, Provider={Provider}",
                result.OrderCode, request.PaymentProvider);

            return ApiResponse<PaymentLinkResponse>.Success(result, "Tạo đường dẫn thanh toán thành công.");
        }
    }
}
