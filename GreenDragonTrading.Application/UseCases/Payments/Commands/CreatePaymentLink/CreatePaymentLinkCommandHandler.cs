using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Payments.Commands.CreatePaymentLink
{
    /// <summary>
    /// Handler for CreatePaymentLinkCommand
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

            _logger.LogInformation("Creating payment link for UserId={UserId}, SubscriptionId={SubscriptionId}", 
                userId, request.SubscriptionId);

            var result = await _paymentService.CreateVipPaymentAsync(
                userId,
                request.SubscriptionId,
                cancellationToken);

            _logger.LogInformation("Payment link created: OrderCode={OrderCode}", result.OrderCode);

            return ApiResponse<PaymentLinkResponse>.Success(result, "Tạo mã QR thanh toán thành công.");
        }
    }
}
