using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Commands.UpdateSubscriptionStatus
{
    public class UpdateSubscriptionStatusCommandHandler : IRequestHandler<UpdateSubscriptionStatusCommand, ApiResponse<SubscriptionDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<UpdateSubscriptionStatusCommandHandler> _logger;

        public UpdateSubscriptionStatusCommandHandler(
            IUnitOfWork uow,
            ILogger<UpdateSubscriptionStatusCommandHandler> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<ApiResponse<SubscriptionDto>> Handle(
            UpdateSubscriptionStatusCommand request,
            CancellationToken cancellationToken)
        {
            var subscription = await _uow.Subscriptions.GetByIdAsync(request.SubscriptionId, cancellationToken);
            if (subscription == null)
            {
                throw new NotFoundException("Gói đăng ký không tồn tại.");
            }

            subscription.IsActive = subscription.IsActive == CommonStatus.Active
                ? CommonStatus.InActive
                : CommonStatus.Active;
            _uow.Subscriptions.Update(subscription);
            await _uow.SaveChangesAsync(cancellationToken);

            var dto = new SubscriptionDto
            {
                Id = subscription.Id,
                Name = subscription.Name,
                LevelOrder = subscription.LevelOrder,
                MaxWorkspaces = subscription.MaxWorkspaces,
                Price = subscription.Price,
                DurationInDays = subscription.DurationInDays,
                IsActive = subscription.IsActive,
                AllowedModules = JsonDocument.Parse(subscription.AllowedModules).RootElement.Clone()
            };

            _logger.LogInformation(
                "Subscription status toggled: SubscriptionId={SubscriptionId}, IsActive={IsActive}",
                subscription.Id,
                subscription.IsActive);

            return ApiResponse<SubscriptionDto>.Success(dto, "Đổi trạng thái gói đăng ký thành công.");
        }
    }
}
