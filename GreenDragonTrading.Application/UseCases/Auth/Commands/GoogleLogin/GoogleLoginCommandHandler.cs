using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.Common.Utils;
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

            var user = await _uow.Users.GetByEmailAsync(googleUser.Email, cancellationToken);

            if (user == null)
            {
                var username = googleUser.Name.Length > 24
                    ? googleUser.Name[..24]
                    : googleUser.Name;

                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = googleUser.Email,
                    Username = username,
                    HashedPassword = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                    PhoneNumber = null,
                    AvatarUrl = googleUser.PictureUrl,
                    Role = UserRole.User,
                    IsEmailVerified = true,
                    Status = CommonStatus.Active,
                    CreatedAt = DateTimeOffset.UtcNow,
                };

                await _uow.Users.AddAsync(user, cancellationToken);
                await _uow.SaveChangesAsync(cancellationToken);

                await CreateDefaultWorkspacesForUserAsync(user.Id, cancellationToken);

                _logger.LogInformation("New user created via Google: {Email}", user.Email);
            }
            else
            {
                if (user.Status != CommonStatus.Active)
                    throw new BusinessRuleException("Tài khoản đã bị khoá.");

                if (!string.IsNullOrEmpty(googleUser.PictureUrl) && user.AvatarUrl != googleUser.PictureUrl)
                {
                    user.AvatarUrl = googleUser.PictureUrl;
                    await _uow.SaveChangesAsync(cancellationToken);
                }
            }

            var effectiveSubscription = await SubscriptionAccessHelper.GetEffectiveSubscriptionAsync(
                _uow,
                user.Role is UserRole.Admin or UserRole.Staff,
                user.Id,
                cancellationToken);

            var accessToken = _jwtService.GenerateAccessToken(
                user.Id,
                user.Email,
                user.Username,
                user.PhoneNumber,
                user.Role.ToString(),
                effectiveSubscription?.LevelOrder is { } level
                    ? level.GetDisplayName()
                    : SubscriptionLevel.Free.GetDisplayName()
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
                    SubscriptionLevel = effectiveSubscription?.LevelOrder is { } lvl
                        ? lvl.GetDisplayName()
                        : SubscriptionLevel.Free.GetDisplayName(),
                    TelegramChatId = user.TelegramId,
                    IsTelegramLinked = !string.IsNullOrWhiteSpace(user.TelegramId)
                }
            };

            _logger.LogInformation("Google login successful: {Email}", user.Email);
            return ApiResponse<AuthResponse>.Success(response, "Đăng nhập thành công.");
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
                    _logger.LogWarning(
                        ex,
                        "Failed to create default workspace for user {UserId} with type {WorkspaceType}. Google login will continue.",
                        userId,
                        workspaceType);
                }
            }
        }
    }
}
