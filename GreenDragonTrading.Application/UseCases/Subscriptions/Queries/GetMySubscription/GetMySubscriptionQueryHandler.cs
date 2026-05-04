using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Common.Utils;
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
            var effectiveSubscription = await SubscriptionAccessHelper.GetEffectiveSubscriptionAsync(
                _uow,
                _currentUserService.IsAdminOrStaff,
                userId,
                cancellationToken);

            if (_currentUserService.IsAdminOrStaff)
            {
                var adminDto = effectiveSubscription == null
                    ? new UserSubscriptionDto
                    {
                        SubscriptionId = null,
                        SubscriptionName = "Admin",
                        LevelOrder = SubscriptionLevel.Free,
                        MaxWorkspaces = 0,
                        Price = 0,
                        DurationInDays = 0,
                        AllowedModules = JsonDocument.Parse("[]").RootElement.Clone(),
                        StartDate = null,
                        EndDate = null,
                        Status = null,
                        IsActive = true
                    }
                    : new UserSubscriptionDto
                    {
                        SubscriptionId = effectiveSubscription.Id,
                        SubscriptionName = effectiveSubscription.Name,
                        LevelOrder = effectiveSubscription.LevelOrder ?? SubscriptionLevel.Free,
                        MaxWorkspaces = effectiveSubscription.MaxWorkspaces ?? 0,
                        Price = effectiveSubscription.Price ?? 0,
                        DurationInDays = effectiveSubscription.DurationInDays ?? 0,
                        AllowedModules = JsonDocument.Parse(string.IsNullOrWhiteSpace(effectiveSubscription.AllowedModules) ? "[]" : effectiveSubscription.AllowedModules).RootElement.Clone(),
                        StartDate = null,
                        EndDate = null,
                        Status = null,
                        IsActive = true
                    };

                _logger.LogInformation("Successfully retrieved admin subscription information for admin/staff user: {UserId}", userId);
                return ApiResponse<UserSubscriptionDto>.Success(adminDto, "Lấy thông tin gói đăng ký thành công.");
            }

            UserSubscriptionDto dto;

            if (effectiveSubscription == null)
            {
                dto = new UserSubscriptionDto
                {
                    SubscriptionId = null,
                    SubscriptionName = SubscriptionLevel.Free.GetDisplayName(),
                    LevelOrder = SubscriptionLevel.Free,
                    MaxWorkspaces = 1,
                    Price = 0,
                    DurationInDays = 0,
                    AllowedModules = JsonDocument.Parse("[]").RootElement.Clone(),
                    StartDate = null,
                    EndDate = null,
                    Status = null,
                    IsActive = true
                };
            }
            else
            {
                var allActiveSubscriptions = await _uow.UserSubscriptions.GetAllActiveByUserIdAsync(userId, cancellationToken);
                var sameTypeSubscriptions = allActiveSubscriptions
                    .Where(us => us.SubscriptionId == effectiveSubscription.Id)
                    .ToList();

                var hasUserSubscription = sameTypeSubscriptions.Count > 0;

                dto = new UserSubscriptionDto
                {
                    SubscriptionId = effectiveSubscription.Id,
                    SubscriptionName = effectiveSubscription.Name,
                    LevelOrder = effectiveSubscription.LevelOrder ?? SubscriptionLevel.Free,
                    MaxWorkspaces = effectiveSubscription.MaxWorkspaces ?? 0,
                    Price = effectiveSubscription.Price ?? 0,
                    DurationInDays = effectiveSubscription.DurationInDays ?? 0,
                    AllowedModules = JsonDocument.Parse(string.IsNullOrWhiteSpace(effectiveSubscription.AllowedModules) ? "[]" : effectiveSubscription.AllowedModules).RootElement.Clone(),
                    StartDate = hasUserSubscription ? sameTypeSubscriptions.Min(us => us.StartDate) : null,
                    EndDate = hasUserSubscription ? sameTypeSubscriptions.Max(us => us.EndDate) : null,
                    Status = hasUserSubscription ? sameTypeSubscriptions.First().Status.GetDisplayName() : null,
                    IsActive = true
                };
            }

            _logger.LogInformation("Successfully retrieved subscription information for user: {UserId}", userId);
            return ApiResponse<UserSubscriptionDto>.Success(dto, "Lấy thông tin gói đăng ký thành công.");
        }
    }
}
