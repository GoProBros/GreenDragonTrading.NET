using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ApiResponse>
    {
        private readonly IUnitOfWork _uow;
        private readonly IRedisService _redisService;
        private readonly IEmailService _emailService;
        private readonly ILogger<ForgotPasswordCommandHandler> _logger;

        public ForgotPasswordCommandHandler(
            IUnitOfWork uow,
            IRedisService redisService,
            IEmailService emailService,
            ILogger<ForgotPasswordCommandHandler> logger)
        {
            _uow = uow;
            _redisService = redisService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ApiResponse> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _uow.Users.GetByEmailAsync(request.Email, cancellationToken);
                
                if (user == null)
                {
                    _logger.LogWarning("Yêu cầu reset password cho email không tồn tại: {Email}", request.Email);
                    throw new NotFoundException("Không tìm thấy người dùng với email đã cung cấp.");
                }

                var resetToken = GenerateResetToken();

                var redisKey = $"password:reset:{user.Id}";
                await _redisService.SetAsync(redisKey, resetToken, TimeSpan.FromMinutes(15));

                await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken, cancellationToken);

                _logger.LogInformation("Đã gửi OTP reset password cho email: {Email}", request.Email);
                return ApiResponse.Success("Chúng tôi đã gửi link reset password đến email của bạn.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý forgot password cho email: {Email}", request.Email);
                throw;
            }
        }

        private static string GenerateResetToken()
        {
            var random = RandomNumberGenerator.GetInt32(100000, 999999);
            return random.ToString();
        }
    }
}
