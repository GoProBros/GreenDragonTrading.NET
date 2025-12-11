using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.VerifyEmail
{
    public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result>
    {
        private readonly IUnitOfWork _uow;
        private readonly IRedisService _redisService;
        private readonly ILogger<VerifyEmailCommandHandler> _logger;

        public VerifyEmailCommandHandler(
            IUnitOfWork uow,
            IRedisService redisService,
            ILogger<VerifyEmailCommandHandler> logger)
        {
            _uow = uow;
            _redisService = redisService;
            _logger = logger;
        }

        public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var redisKey = $"verify:email:token:{request.Token}";
                var tokenData = await _redisService.GetAsync<Guid>(redisKey);

                if (tokenData == Guid.Empty)
                {
                    throw new UnauthenticatedException("Mã xác thực đã hết hạn hoặc không hợp lệ.");
                }

                var user = await _uow.Users.GetByIdAsync(tokenData, cancellationToken);
                if (user == null)
                {
                    throw new NotFoundException("User id", tokenData);
                }

                if (user.IsEmailVerified)
                {
                    throw new BusinessRuleException("Email đã được xác thực trước đó.");
                }

                user.IsEmailVerified = true;
                await _uow.SaveChangesAsync(cancellationToken);

                await _redisService.RemoveAsync(redisKey);

                _logger.LogInformation("Xác thực email thành công: {Email}", user.Email);
                return new Result(true, "Xác thực email thành công. Bạn có thể đăng nhập.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống.");
                throw;
            }
        }
    }
}
