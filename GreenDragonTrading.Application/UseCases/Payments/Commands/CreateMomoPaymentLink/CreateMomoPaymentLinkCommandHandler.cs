using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Payments.Commands.CreateMomoPaymentLink
{
    /// <summary>
    /// Handles <see cref="CreateMomoPaymentLinkCommand"/>.
    /// Delegates to <see cref="IPaymentService.CreateMomoVipPaymentAsync"/> and wraps
    /// the result in a standard <see cref="ApiResponse{T}"/>.
    /// </summary>
    public class CreateMomoPaymentLinkCommandHandler
        : IRequestHandler<CreateMomoPaymentLinkCommand, ApiResponse<MomoPaymentLinkResponse>>
    {
        private readonly IPaymentService _paymentService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CreateMomoPaymentLinkCommandHandler> _logger;

        public CreateMomoPaymentLinkCommandHandler(
            IPaymentService paymentService,
            ICurrentUserService currentUserService,
            ILogger<CreateMomoPaymentLinkCommandHandler> logger)
        {
            _paymentService = paymentService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ApiResponse<MomoPaymentLinkResponse>> Handle(
            CreateMomoPaymentLinkCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetRequiredUserId();

            _logger.LogInformation(
                "Creating Momo payment link for UserId={UserId}, SubscriptionId={SubscriptionId}",
                userId, request.SubscriptionId);

            var result = await _paymentService.CreateMomoVipPaymentAsync(
                userId,
                request.SubscriptionId,
                cancellationToken);

            _logger.LogInformation("Momo payment link created: OrderId={OrderId}", result.OrderId);

            return ApiResponse<MomoPaymentLinkResponse>.Success(result, "Tạo đường dẫn thanh toán Momo thành công.");
        }
    }
}
