using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result>
    {
        private readonly IUnitOfWork _uow;
        private readonly IEmailService _emailService;
        private readonly IRedisService _redisService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RegisterCommandHandler> _logger;

        public RegisterCommandHandler(
            IUnitOfWork uow,
            IEmailService emailService,
            IRedisService redisService,
            IConfiguration configuration,
            ILogger<RegisterCommandHandler> logger)
        {
            _uow = uow;
            _emailService = emailService;
            _redisService = redisService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (await _uow.Users.EmailExistsAsync(request.Email, cancellationToken))
                {
                    throw new ConflictException("Email này đã được đăng kí.");
                }

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email,
                    HashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Username = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    Role = UserRole.User,
                    IsEmailVerified = false,
                    Status = CommonStatus.Active,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                await _uow.Users.AddAsync(user, cancellationToken);
                await _uow.SaveChangesAsync(cancellationToken);

                var verificationToken = GenerateSecureToken();
                
                var redisKey = $"verify:email:token:{verificationToken}";
                await _redisService.SetAsync(redisKey, user.Id, TimeSpan.FromHours(1));

                var baseUrl = _configuration["AppSettings:BaseUrl"];
                var verificationUrl = $"{baseUrl}/api/auth/verify-email";

                await _emailService.SendVerificationEmailAsync(user.Email, verificationToken, verificationUrl, cancellationToken);

                _logger.LogInformation("Đăng kí thành công: {Email}", request.Email);
                return new Result(true,"Đăng kí thành công. Kiểm tra email để xác thực.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi này do hệ thống, chưa handle.");
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
