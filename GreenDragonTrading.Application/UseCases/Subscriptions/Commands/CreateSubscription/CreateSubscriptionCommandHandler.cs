using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
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
            if(request.LevelOrder < SubscriptionLevel.Advanced || request.LevelOrder > SubscriptionLevel.VipThree)
            {
                _logger.LogWarning("Invalid subscription level: {LevelOrder}", request.LevelOrder);
                return ApiResponse<SubscriptionDto>.Failure("Cấp độ đăng ký không hợp lệ.");
            }

            var subscription = new Subscription
            {
                Name = request.Name,
                LevelOrder = request.LevelOrder,
                MaxWorkspaces = request.MaxWorkspaces,
                Price = request.Price,
                DurationInDays = request.DurationInDays,
                IsActive = request.IsActive,
                IsFree = false,
                IsAdmin = false,
                AllowedModules = request.AllowedModules.GetRawText()
            };

            await _uow.Subscriptions.AddAsync(subscription, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Subscription created: Id={Id}", subscription.Id);

            var dto = new SubscriptionDto
            {
                Id = subscription.Id,
                Name = subscription.Name,
                LevelOrder = subscription.LevelOrder ?? default,
                MaxWorkspaces = subscription.MaxWorkspaces ?? 0,
                Price = subscription.Price ?? 0,
                DurationInDays = subscription.DurationInDays ?? 0,
                IsActive = subscription.IsActive,
                AllowedModules = request.AllowedModules
            };

            return ApiResponse<SubscriptionDto>.Success(dto, "Tạo gói đăng ký thành công.");
        }
    }
}
