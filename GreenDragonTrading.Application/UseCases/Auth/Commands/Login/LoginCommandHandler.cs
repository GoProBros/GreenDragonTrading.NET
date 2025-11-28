using GreenDragonTrading.Application.DTOs.Auth;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IRedisService _redisService;

    public LoginCommandHandler(
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

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Verify password
        if (!_passwordHasher.Verify(request.Password, user.HashedPassword))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

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
