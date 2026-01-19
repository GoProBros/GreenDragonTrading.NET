using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Payments.Commands.CancelPaymentLink
{
    public class CancelPaymentLinkCommandHandler : IRequestHandler<CancelPaymentLinkCommand, ApiResponse<PaymentInformationResponse>>
    {
        private readonly IPaymentService _paymentService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtService _jwtService;
        private readonly ILogger<CancelPaymentLinkCommandHandler> _logger;

        public CancelPaymentLinkCommandHandler(
            IPaymentService paymentService,
            IHttpContextAccessor httpContextAccessor,
            IJwtService jwtService,
            ILogger<CancelPaymentLinkCommandHandler> logger)
        {
            _paymentService = paymentService;
            _httpContextAccessor = httpContextAccessor;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<ApiResponse<PaymentInformationResponse>> Handle(
            CancelPaymentLinkCommand request,
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
            _logger.LogInformation("Cancelling payment link for UserId={UserId}, OrderCode={OrderCode}, Reason={Reason}",
                tokenInfo.UserId, request.orderCode, request.reason);
            var result = await _paymentService.CancelPaymentAsync(
                request.orderCode,
                request.reason,
                cancellationToken);
            return ApiResponse<PaymentInformationResponse>.Success(result, "Hủy link thanh toán thành công.");
        }
    }
}
