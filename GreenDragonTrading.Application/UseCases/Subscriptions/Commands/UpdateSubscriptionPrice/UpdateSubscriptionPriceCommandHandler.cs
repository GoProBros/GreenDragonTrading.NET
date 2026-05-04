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

            var updatedFields = new List<string>();

            if (request.Price.HasValue)
            {
                subscription.Price = request.Price.Value;
                updatedFields.Add("Price");
            }

            if (request.AllowedModules.HasValue)
            {
                subscription.AllowedModules = request.AllowedModules.Value.GetRawText();
                updatedFields.Add("AllowedModules");
            }

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
                "Subscription updated: SubscriptionId={SubscriptionId}, UpdatedFields={UpdatedFields}, Price={Price}",
                subscription.Id,
                string.Join(",", updatedFields),
                subscription.Price);

            return ApiResponse<SubscriptionDto>.Success(dto, "Cập nhật gói đăng ký thành công.");
        }
    }
}
