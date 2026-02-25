using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Queries.GetMySubscription
{
    /// <summary>
    /// Handler for GetMySubscriptionQuery
    /// </summary>
    public class GetMySubscriptionQueryHandler : IRequestHandler<GetMySubscriptionQuery, ApiResponse<UserSubscriptionDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetMySubscriptionQueryHandler> _logger;

        public GetMySubscriptionQueryHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUserService,
            ILogger<GetMySubscriptionQueryHandler> logger)
        {
            _uow = uow;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ApiResponse<UserSubscriptionDto>> Handle(GetMySubscriptionQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetRequiredUserId();

            var userSubscription = await _uow.UserSubscriptions.GetActiveSubscriptionAsync(userId, cancellationToken);

            UserSubscriptionDto dto;

            if (userSubscription == null)
            {
                dto = new UserSubscriptionDto
                {
                    SubscriptionId = null,
                    SubscriptionName = SubscriptionLevel.Free.GetDisplayName(),
                    LevelOrder = SubscriptionLevel.Free,
                    MaxWorkspaces = 1,
                    Price = 0,
                    DurationInDays = 0,
                    AllowedModules = JsonDocument.Parse("[]").RootElement,
                    StartDate = null,
                    EndDate = null,
                    Status = null,
                    IsActive = false
                };
            }
            else
            {
                var subscription = userSubscription.Subscription;

                var allActiveSubscriptions = await _uow.UserSubscriptions.GetAllActiveByUserIdAsync(userId, cancellationToken);
                var sameTypeSubscriptions = allActiveSubscriptions
                    .Where(us => us.SubscriptionId == subscription.Id)
                    .ToList();

                var earliestStartDate = sameTypeSubscriptions.Min(us => us.StartDate);
                var latestEndDate = sameTypeSubscriptions.Max(us => us.EndDate);

                dto = new UserSubscriptionDto
                {
                    SubscriptionId = subscription.Id,
                    SubscriptionName = subscription.Name,
                    LevelOrder = subscription.LevelOrder,
                    MaxWorkspaces = subscription.MaxWorkspaces,
                    Price = subscription.Price,
                    DurationInDays = subscription.DurationInDays,
                    AllowedModules = JsonDocument.Parse(subscription.AllowedModules).RootElement,
                    StartDate = earliestStartDate,
                    EndDate = latestEndDate,
                    Status = userSubscription.Status.GetDisplayName(),
                    IsActive = true
                };
            }

            _logger.LogInformation("Successfully retrieved subscription information for user: {UserId}", userId);
            return ApiResponse<UserSubscriptionDto>.Success(dto, "Lấy thông tin gói đăng ký thành công.");
        }
    }
}
