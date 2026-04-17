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
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse>
    {
        private readonly IUnitOfWork _uow;
        private readonly IEmailService _emailService;
        private readonly IRedisService _redisService;
        private readonly IConfiguration _configuration;
        private readonly IWorkspaceDuplicationService _workspaceDuplicationService;
        private readonly ILogger<RegisterCommandHandler> _logger;

        public RegisterCommandHandler(
            IUnitOfWork uow,
            IEmailService emailService,
            IRedisService redisService,
            IConfiguration configuration,
            IWorkspaceDuplicationService workspaceDuplicationService,
            ILogger<RegisterCommandHandler> logger)
        {
            _uow = uow;
            _emailService = emailService;
            _redisService = redisService;
            _configuration = configuration;
            _workspaceDuplicationService = workspaceDuplicationService;
            _logger = logger;
        }

        public async Task<ApiResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
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

                // Create both Web and Mobile default workspaces for the new user
                await CreateDefaultWorkspacesForUserAsync(user.Id, cancellationToken);

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

                _logger.LogInformation("Registration successful: {Email}", request.Email);
                return ApiResponse.Success("Đăng kí thành công. Vui lòng kiểm tra email để xác thực tài khoản.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System error during registration.");
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

        /// <summary>
        /// Creates both Web and Mobile default workspaces for the newly registered user.
        /// </summary>
        private async Task CreateDefaultWorkspacesForUserAsync(Guid userId, CancellationToken cancellationToken)
        {
            var defaultTypes = new[] { WorkspaceType.Web, WorkspaceType.Mobile };

            foreach (var workspaceType in defaultTypes)
            {
                try
                {
                    var systemDefaultWorkspace = await _uow.Workspaces.GetSystemDefaultWorkspaceAsync(workspaceType, cancellationToken);

                    if (systemDefaultWorkspace == null)
                    {
                        _logger.LogWarning(
                            "System default workspace not found for type {WorkspaceType}. Skipping default workspace creation for user {UserId}",
                            workspaceType,
                            userId);
                        continue;
                    }

                    await _workspaceDuplicationService.DuplicateWorkspaceAsync(
                        systemDefaultWorkspace,
                        userId,
                        "(Sao chép)",
                        cancellationToken);

                    _logger.LogInformation(
                        "Default workspace created successfully for user {UserId} with type {WorkspaceType}",
                        userId,
                        workspaceType);
                }
                catch (Exception ex)
                {
                    // Log warning but don't fail registration if workspace creation fails
                    _logger.LogWarning(
                        ex,
                        "Failed to create default workspace for user {UserId} with type {WorkspaceType}. User registration will continue.",
                        userId,
                        workspaceType);
                }
            }
        }
    }
}
