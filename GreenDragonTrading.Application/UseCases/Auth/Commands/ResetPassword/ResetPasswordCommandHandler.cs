using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ApiResponse>
    {
        private readonly IUnitOfWork _uow;
        private readonly IRedisService _redisService;
        private readonly ILogger<ResetPasswordCommandHandler> _logger;

        public ResetPasswordCommandHandler(
            IUnitOfWork uow,
            IRedisService redisService,
            ILogger<ResetPasswordCommandHandler> logger)
        {
            _uow = uow;
            _redisService = redisService;
            _logger = logger;
        }

        public async Task<ApiResponse> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _uow.Users.GetByEmailAsync(request.Email, cancellationToken);
                if (user == null)
                {
                    throw new NotFoundException("Email không tồn tại.");
                }

                var redisKey = $"password:reset:{user.Id}";
                var storedToken = await _redisService.GetAsync<string>(redisKey);

                if (string.IsNullOrEmpty(storedToken))
                {
                    throw new BusinessRuleException("Mã reset không hợp lệ hoặc đã hết hạn.");
                }

                if (storedToken != request.ResetToken)
                {
                    throw new BusinessRuleException("Mã reset không đúng.");
                }

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.HashedPassword = hashedPassword;

                _uow.Users.Update(user);
                await _uow.SaveChangesAsync(cancellationToken);

                await _redisService.RemoveAsync(redisKey);

                _logger.LogInformation("Đã reset password thành công cho user: {UserId}", user.Id);
                return ApiResponse.Success("Đổi mật khẩu thành công. Vui lòng đăng nhập lại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi reset password cho email: {Email}", request.Email);
                throw;
            }
        }
    }
}
