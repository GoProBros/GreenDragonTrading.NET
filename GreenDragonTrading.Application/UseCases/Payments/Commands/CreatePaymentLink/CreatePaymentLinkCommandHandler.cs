using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Payments.Commands.CreatePaymentLink
{
    public class CreatePaymentLinkCommandHandler : IRequestHandler<CreatePaymentLinkCommand, ApiResponse<PaymentLinkResponse>>
    {
        private readonly IPaymentService _paymentService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtService _jwtService;
        private readonly ILogger<CreatePaymentLinkCommandHandler> _logger;

        public CreatePaymentLinkCommandHandler(
            IPaymentService paymentService,
            IHttpContextAccessor httpContextAccessor,
            IJwtService jwtService,
            ILogger<CreatePaymentLinkCommandHandler> logger)
        {
            _paymentService = paymentService;
            _httpContextAccessor = httpContextAccessor;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<ApiResponse<PaymentLinkResponse>> Handle(
            CreatePaymentLinkCommand request,
            CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new UnauthenticatedException("Không tìm thấy HTTP context.");
            }

            var authHeaderValue = httpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeaderValue) || !authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthenticatedException("Không tìm thấy Authorization header.");
            }

            var accessToken = authHeaderValue.Substring("Bearer ".Length).Trim();
            var tokenInfo = _jwtService.GetTokenInfo(accessToken);
            if (tokenInfo == null)
            {
                throw new UnauthenticatedException("Access token không hợp lệ.");
            }

            _logger.LogInformation("Creating payment link for UserId={UserId}, SubscriptionId={SubscriptionId}", 
                tokenInfo.UserId, request.SubscriptionId);

            var result = await _paymentService.CreateVipPaymentAsync(
                tokenInfo.UserId,
                request.SubscriptionId,
                cancellationToken);

            _logger.LogInformation("Payment link created: OrderCode={OrderCode}", result.OrderCode);

            return ApiResponse<PaymentLinkResponse>.Success(result, "Tạo mã QR thanh toán thành công.");
        }
    }
}
