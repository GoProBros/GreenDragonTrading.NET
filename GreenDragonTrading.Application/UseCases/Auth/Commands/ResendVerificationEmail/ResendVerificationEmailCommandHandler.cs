using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.ResendVerificationEmail
{
    public class ResendVerificationEmailCommandHandler : IRequestHandler<ResendVerificationEmailCommand, ApiResponse>
    {
        private readonly IUnitOfWork _uow;
        private readonly IRedisService _redisService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ResendVerificationEmailCommandHandler> _logger;

        public ResendVerificationEmailCommandHandler(
            IUnitOfWork uow,
            IRedisService redisService,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<ResendVerificationEmailCommandHandler> logger)
        {
            _uow = uow;
            _redisService = redisService;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ApiResponse> Handle(ResendVerificationEmailCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _uow.Users.GetByEmailAsync(request.Email, cancellationToken);
                if (user == null)
                {
                    throw new NotFoundException("Email không tồn tại.");
                }

                if (user.Status != CommonStatus.Active)
                {
                    throw new BusinessRuleException("Tài khoản đã bị khoá.");
                }

                if (user.IsEmailVerified)
                {
                    throw new BusinessRuleException("Email đã được xác thực.");
                }

                var userTokenKey = $"verify:email:user:{user.Id}";
                var currentToken = await _redisService.GetAsync<string>(userTokenKey);

                if (!string.IsNullOrWhiteSpace(currentToken))
                {
                    await _redisService.RemoveAsync($"verify:email:token:{currentToken}");
                    await _redisService.RemoveAsync(userTokenKey);
                }

                var verificationToken = GenerateSecureToken();
                var tokenRedisKey = $"verify:email:token:{verificationToken}";
                var expiry = TimeSpan.FromHours(1);

                await _redisService.SetAsync(tokenRedisKey, user.Id, expiry);
                await _redisService.SetAsync(userTokenKey, verificationToken, expiry);

                var verificationUrl = _configuration["AppSettings:EmailVerificationUrl"];
                if (string.IsNullOrWhiteSpace(verificationUrl))
                {
                    var baseUrl = _configuration["AppSettings:BaseUrl"];
                    verificationUrl = $"{baseUrl}/api/v1/auth/verify-email?token={{token}}";
                }

                await _emailService.SendVerificationEmailAsync(user.Email, verificationToken, verificationUrl, cancellationToken);

                _logger.LogInformation("Resent verification email: {Email}", request.Email);
                return ApiResponse.Success("Đã gửi lại email xác thực. Vui lòng kiểm tra hộp thư của bạn.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System error during resending verification email: {Email}", request.Email);
                throw;
            }
        }

        private static string GenerateSecureToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
    }
}
