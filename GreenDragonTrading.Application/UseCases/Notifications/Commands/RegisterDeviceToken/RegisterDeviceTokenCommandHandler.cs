using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Notifications.Commands.RegisterDeviceToken
{
    public class RegisterDeviceTokenCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService)
        : IRequestHandler<RegisterDeviceTokenCommand, ApiResponse>
    {
        public async Task<ApiResponse> Handle(
            RegisterDeviceTokenCommand request,
            CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetRequiredUserId();

            var existing = await uow.UserPushTokens.FindByUserAndTokenAsync(
                userId, request.ExpoPushToken, cancellationToken);

            if (existing != null)
            {
                existing.UpdatedAt = DateTimeOffset.UtcNow;
                uow.UserPushTokens.Update(existing);
            }
            else
            {
                await uow.UserPushTokens.AddAsync(new UserPushToken
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Token = request.ExpoPushToken,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                }, cancellationToken);
            }

            await uow.SaveChangesAsync(cancellationToken);

            return ApiResponse.Success("Đăng ký thiết bị thành công");
        }
    }
}
