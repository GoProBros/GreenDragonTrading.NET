using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Commands.UpdateSubscriptionPrice
{
    public class UpdateSubscriptionPriceCommandHandler : IRequestHandler<UpdateSubscriptionPriceCommand, ApiResponse<SubscriptionDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<UpdateSubscriptionPriceCommandHandler> _logger;

        public UpdateSubscriptionPriceCommandHandler(
            IUnitOfWork uow,
            ILogger<UpdateSubscriptionPriceCommandHandler> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<ApiResponse<SubscriptionDto>> Handle(
            UpdateSubscriptionPriceCommand request,
            CancellationToken cancellationToken)
        {
            var subscription = await _uow.Subscriptions.GetByIdAsync(request.SubscriptionId, cancellationToken);
            if (subscription == null)
            {
                throw new NotFoundException("Gói đăng ký không tồn tại.");
            }

            subscription.Price = request.Price;
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
                "Subscription price updated: SubscriptionId={SubscriptionId}, Price={Price}",
                subscription.Id,
                subscription.Price);

            return ApiResponse<SubscriptionDto>.Success(dto, "Cập nhật giá gói đăng ký thành công.");
        }
    }
}
