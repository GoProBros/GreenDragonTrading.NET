using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.Auth.Queries.GetMe
{
    /// <summary>
    /// Handler for GetMeQuery
    /// </summary>
    public class GetMeQueryHandler : IRequestHandler<GetMeQuery, ApiResponse<UserDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetMeQueryHandler> _logger;

        public GetMeQueryHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUserService,
            ILogger<GetMeQueryHandler> logger)
        {
            _uow = uow;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ApiResponse<UserDto>> Handle(GetMeQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetRequiredUserId();

            var user = await _uow.Users.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                throw new NotFoundException("Người dùng không tồn tại.");
            }

            var subscriptionLevel = await _uow.UserSubscriptions.GetActiveSubscriptionAsync(user.Id, cancellationToken);
            var effectiveSubscription = subscriptionLevel?.Subscription;

            if (user.Role is UserRole.Admin or UserRole.Staff)
            {
                effectiveSubscription = await _uow.Subscriptions.GetHighestActiveAsync(cancellationToken)
                    ?? effectiveSubscription;
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.Username,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.GetDisplayName(),
                IsEmailVerified = user.IsEmailVerified,
                SubscriptionLevel = effectiveSubscription?.LevelOrder.GetDisplayName() ?? SubscriptionLevel.Free.GetDisplayName(),
                AllowedModules = ParseAllowedModuleNames(effectiveSubscription?.AllowedModules),
                TelegramChatId = user.TelegramId,
                IsTelegramLinked = !string.IsNullOrWhiteSpace(user.TelegramId)
            };

            _logger.LogInformation("Successfully retrieved user information: {UserId}", userId);
            return ApiResponse<UserDto>.Success(userDto, "Lấy thông tin người dùng thành công.");
        }

        private static List<string> ParseAllowedModuleNames(string? allowedModulesJson)
        {
            if (string.IsNullOrWhiteSpace(allowedModulesJson))
            {
                return [];
            }

            try
            {
                using var document = JsonDocument.Parse(allowedModulesJson);
                if (document.RootElement.ValueKind != JsonValueKind.Array)
                {
                    return [];
                }

                return document.RootElement
                    .EnumerateArray()
                    .Where(x => x.ValueKind == JsonValueKind.String)
                    .Select(x => x.GetString())
                    .OfType<string>()
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch
            {
                return [];
            }
        }
    }
}
