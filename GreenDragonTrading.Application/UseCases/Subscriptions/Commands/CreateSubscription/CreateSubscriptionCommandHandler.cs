using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Commands.CreateSubscription
{
    public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, ApiResponse<SubscriptionDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<CreateSubscriptionCommandHandler> _logger;

        public CreateSubscriptionCommandHandler(IUnitOfWork uow, ILogger<CreateSubscriptionCommandHandler> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<ApiResponse<SubscriptionDto>> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating subscription: Name={Name}, LevelOrder={LevelOrder}", request.Name, request.LevelOrder);

            var subscription = new Subscription
            {
                Name = request.Name,
                LevelOrder = request.LevelOrder,
                MaxWorkspaces = request.MaxWorkspaces,
                Price = request.Price,
                DurationInDays = request.DurationInDays,
                IsActive = request.IsActive,
                AllowedModules = request.AllowedModules.GetRawText()
            };

            await _uow.Subscriptions.AddAsync(subscription, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Subscription created: Id={Id}", subscription.Id);

            var dto = new SubscriptionDto
            {
                Id = subscription.Id,
                Name = subscription.Name,
                LevelOrder = subscription.LevelOrder,
                MaxWorkspaces = subscription.MaxWorkspaces,
                Price = subscription.Price,
                DurationInDays = subscription.DurationInDays,
                IsActive = subscription.IsActive,
                AllowedModules = request.AllowedModules
            };

            return ApiResponse<SubscriptionDto>.Success(dto, "Tạo gói đăng ký thành công.");
        }
    }
}
