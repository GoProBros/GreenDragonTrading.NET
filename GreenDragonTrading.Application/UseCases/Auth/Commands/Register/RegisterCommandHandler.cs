using GreenDragonTrading.Application.DTOs.Auth;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IRedisService _redisService;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IRedisService redisService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _redisService = redisService;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if username exists
        if (await _userRepository.UsernameExistsAsync(request.Username, cancellationToken))
        {
            throw new InvalidOperationException("Username already exists");
        }

        // Check if email exists
        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new InvalidOperationException("Email already exists");
        }

        // Create user
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            HashedPassword = _passwordHasher.Hash(request.Password),
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, cancellationToken);

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Store refresh token in Redis
        var refreshTokenExpiration = TimeSpan.FromDays(_jwtService.GetRefreshTokenExpirationDays());
        await _redisService.SetRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiration);

        return new AuthResponse(
            user.Id,
            user.Username,
            user.Email,
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(_jwtService.GetAccessTokenExpirationMinutes())
        );
    }
}
