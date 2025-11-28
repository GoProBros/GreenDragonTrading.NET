using GreenDragonTrading.Application.DTOs.Auth;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IRedisService _redisService;

    public RefreshTokenCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IRedisService redisService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _redisService = redisService;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Validate access token and extract user id
        var userId = _jwtService.ValidateAccessToken(request.AccessToken);

        if (userId == null)
        {
            throw new UnauthorizedException("Invalid access token");
        }

        // Validate refresh token from Redis
        var isValidRefreshToken = await _redisService.ValidateRefreshTokenAsync(userId.Value, request.RefreshToken);

        if (!isValidRefreshToken)
        {
            throw new UnauthorizedException("Invalid or expired refresh token");
        }

        // Get user
        var user = await _unitOfWork.Users.GetByIdAsync(userId.Value, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId.Value);
        }

        // Generate new tokens
        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        // Update refresh token in Redis
        var refreshTokenExpiration = TimeSpan.FromDays(_jwtService.GetRefreshTokenExpirationDays());
        await _redisService.SetRefreshTokenAsync(user.Id, newRefreshToken, refreshTokenExpiration);

        return new AuthResponse(
            user.Id,
            user.Username,
            user.Email,
            newAccessToken,
            newRefreshToken,
            DateTime.UtcNow.AddMinutes(_jwtService.GetAccessTokenExpirationMinutes())
        );
    }
}
