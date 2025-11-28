using GreenDragonTrading.Application.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly IRedisService _redisService;

    public LogoutCommandHandler(IRedisService redisService)
    {
        _redisService = redisService;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await _redisService.RemoveRefreshTokenAsync(request.UserId);
        return true;
    }
}
