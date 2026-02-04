using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
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
        private readonly IJwtService _jwtService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<GetMySubscriptionQueryHandler> _logger;

        public GetMySubscriptionQueryHandler(
            IUnitOfWork uow,
            IJwtService jwtService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<GetMySubscriptionQueryHandler> logger)
        {
            _uow = uow;
            _jwtService = jwtService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<ApiResponse<UserSubscriptionDto>> Handle(GetMySubscriptionQuery request, CancellationToken cancellationToken)
        {
            // Get access token from Authorization header
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new UnauthenticatedException("Không tìm thấy HTTP context.");
            }

            var authHeaderValue = httpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeaderValue) || !authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthenticatedException("Không tìm thấy Authorization header.");
            }

            var accessToken = authHeaderValue.Substring("Bearer ".Length).Trim();

            var tokenInfo = _jwtService.GetTokenInfo(accessToken);
            if (tokenInfo == null)
            {
                throw new UnauthenticatedException("Access token không hợp lệ.");
            }

            var user = await _uow.Users.GetByIdAsync(tokenInfo.UserId, cancellationToken);
            if (user == null)
            {
                throw new NotFoundException("Người dùng không tồn tại.");
            }

            var userSubscription = await _uow.UserSubscriptions.GetActiveSubscriptionAsync(user.Id, cancellationToken);

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

                var allActiveSubscriptions = await _uow.UserSubscriptions.GetAllActiveByUserIdAsync(user.Id, cancellationToken);
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

            _logger.LogInformation("Successfully retrieved subscription information for user: {UserId}", tokenInfo.UserId);
            return ApiResponse<UserSubscriptionDto>.Success(dto, "Lấy thông tin gói đăng ký thành công.");
        }
    }
}
