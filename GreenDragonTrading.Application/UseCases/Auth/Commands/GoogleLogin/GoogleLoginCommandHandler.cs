using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.GoogleLogin
{
    public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, ApiResponse<AuthResponse>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IJwtService _jwtService;
        private readonly IRedisService _redisService;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IWorkspaceDuplicationService _workspaceDuplicationService;
        private readonly ILogger<GoogleLoginCommandHandler> _logger;
        private readonly JwtOptions _jwtOptions;

        public GoogleLoginCommandHandler(
            IUnitOfWork uow,
            IJwtService jwtService,
            IRedisService redisService,
            IGoogleAuthService googleAuthService,
            IWorkspaceDuplicationService workspaceDuplicationService,
            ILogger<GoogleLoginCommandHandler> logger,
            IOptions<JwtOptions> jwtOptions)
        {
            _uow = uow;
            _jwtService = jwtService;
            _redisService = redisService;
            _googleAuthService = googleAuthService;
            _workspaceDuplicationService = workspaceDuplicationService;
            _logger = logger;
            _jwtOptions = jwtOptions.Value;
        }

        public async Task<ApiResponse<AuthResponse>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
        {
            // Verify the Google ID token server-side
            GoogleUserInfo googleUser;
            try
            {
                googleUser = await _googleAuthService.VerifyIdTokenAsync(request.IdToken, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalid Google ID token");
                throw new UnauthenticatedException("Google token không hợp lệ.");
            }

            // Find existing user or create a new one
            var user = await _uow.Users.GetByEmailAsync(googleUser.Email, cancellationToken);

            if (user == null)
            {
                // Derive a unique username from the Google display name
                var username = googleUser.Name.Length > 24
                    ? googleUser.Name[..24]
                    : googleUser.Name;

                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = googleUser.Email,
                    Username = username,
                    // Google users have no password — store a random unguessable hash
                    HashedPassword = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                    PhoneNumber = string.Empty,
                    AvatarUrl = googleUser.PictureUrl,
                    Role = UserRole.User,
                    IsEmailVerified = true, // Google already verified the email
                    Status = CommonStatus.Active,
                    CreatedAt = DateTimeOffset.UtcNow,
                };

                await _uow.Users.AddAsync(user, cancellationToken);
                await _uow.SaveChangesAsync(cancellationToken);

                // Create a copy of the system default workspace for the new user
                await CreateDefaultWorkspaceForUserAsync(user.Id, cancellationToken);

                _logger.LogInformation("New user created via Google: {Email}", user.Email);
            }
            else
            {
                if (user.Status != CommonStatus.Active)
                    throw new BusinessRuleException("Tài khoản đã bị khoá.");

                // Update avatar if Google provides a newer one
                if (!string.IsNullOrEmpty(googleUser.PictureUrl) && user.AvatarUrl != googleUser.PictureUrl)
                {
                    user.AvatarUrl = googleUser.PictureUrl;
                    await _uow.SaveChangesAsync(cancellationToken);
                }
            }

            var subscriptionLevel = await _uow.UserSubscriptions.GetActiveSubscriptionAsync(user.Id, cancellationToken);

            var accessToken = _jwtService.GenerateAccessToken(
                user.Id,
                user.Email,
                user.Username,
                user.PhoneNumber,
                user.Role.ToString(),
                subscriptionLevel?.Subscription.LevelOrder.GetDisplayName() ?? SubscriptionLevel.Free.GetDisplayName()
            );
            var refreshToken = _jwtService.GenerateRefreshToken();

            var redisKey = $"refresh:token:{refreshToken}";
            await _redisService.SetAsync(redisKey, user.Id, TimeSpan.FromDays(_jwtOptions.RefreshTokenExpirationDays));

            var response = new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes),
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.Username,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role.GetDisplayName(),
                    IsEmailVerified = user.IsEmailVerified,
                    SubscriptionLevel = subscriptionLevel?.Subscription.LevelOrder.GetDisplayName() ?? SubscriptionLevel.Free.GetDisplayName()
                }
            };

            _logger.LogInformation("Google login successful: {Email}", user.Email);
            return ApiResponse<AuthResponse>.Success(response, "Đăng nhập thành công.");
        }

        /// <summary>
        /// Creates a copy of the system default workspace for the newly registered user.
        /// </summary>
        private async Task CreateDefaultWorkspaceForUserAsync(Guid userId, CancellationToken cancellationToken)
        {
            try
            {
                var systemDefaultWorkspace = await _uow.Workspaces.GetSystemDefaultWorkspaceAsync(cancellationToken);

                if (systemDefaultWorkspace == null)
                {
                    _logger.LogWarning("System default workspace not found. Skipping default workspace creation for user {UserId}", userId);
                    return;
                }

                await _workspaceDuplicationService.DuplicateWorkspaceAsync(
                    systemDefaultWorkspace,
                    userId,
                    "(Sao chép)",
                    cancellationToken);

                _logger.LogInformation("Default workspace created successfully for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create default workspace for user {UserId}. Google login will continue.", userId);
            }
        }
    }
}
