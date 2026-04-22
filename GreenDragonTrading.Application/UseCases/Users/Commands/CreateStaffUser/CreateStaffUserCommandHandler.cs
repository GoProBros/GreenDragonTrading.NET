using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace GreenDragonTrading.Application.UseCases.Users.Commands.CreateStaffUser;

/// <summary>
/// Handler for CreateStaffUserCommand.
/// </summary>
public class CreateStaffUserCommandHandler : IRequestHandler<CreateStaffUserCommand, ApiResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;
    private readonly IRedisService _redisService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CreateStaffUserCommandHandler> _logger;

    public CreateStaffUserCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        IEmailService emailService,
        IRedisService redisService,
        IConfiguration configuration,
        ILogger<CreateStaffUserCommandHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _emailService = emailService;
        _redisService = redisService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ApiResponse> Handle(CreateStaffUserCommand request, CancellationToken cancellationToken)
    {
        var normalizedPhoneNumber = request.PhoneNumber.Trim();

        if (_currentUserService.Role != nameof(UserRole.Admin))
        {
            throw new AccessDeniedException("Chỉ Admin mới có quyền tạo tài khoản Staff.");
        }

        if (await _uow.Users.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new ConflictException("Email này đã được đăng ký.");
        }

        if (await _uow.Users.PhoneNumberExistsAsync(normalizedPhoneNumber, cancellationToken))
        {
            throw new ConflictException("Số điện thoại này đã được đăng ký.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            HashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Username = request.FullName,
            PhoneNumber = normalizedPhoneNumber,
            AvatarUrl = request.AvatarUrl,
            Role = UserRole.Staff,
            IsEmailVerified = !request.RequireEmailVerification,
            Status = CommonStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _uow.Users.AddAsync(user, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        if (request.RequireEmailVerification)
        {
            var verificationToken = GenerateSecureToken();
            var redisKey = $"verify:email:token:{verificationToken}";
            await _redisService.SetAsync(redisKey, user.Id, TimeSpan.FromHours(1));

            var verificationUrl = _configuration["AppSettings:EmailVerificationUrl"];
            if (string.IsNullOrWhiteSpace(verificationUrl))
            {
                var baseUrl = _configuration["AppSettings:BaseUrl"];
                verificationUrl = $"{baseUrl}/api/v1/auth/verify-email?token={{token}}";
            }

            await _emailService.SendVerificationEmailAsync(user.Email, verificationToken, verificationUrl, cancellationToken);

            _logger.LogInformation("Staff account created with verification required: {Email}", user.Email);
            return ApiResponse.Success("Tạo tài khoản staff thành công. Email xác thực đã được gửi.");
        }

        _logger.LogInformation("Staff account created without verification: {Email}", user.Email);
        return ApiResponse.Success("Tạo tài khoản staff thành công.");
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