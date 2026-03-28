using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.Users.Queries.GetUserDetail;

/// <summary>
/// Handler for GetUserDetailQuery.
/// </summary>
public class GetUserDetailQueryHandler : IRequestHandler<GetUserDetailQuery, ApiResponse<UserManagementDetailDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetUserDetailQueryHandler> _logger;

    public GetUserDetailQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        ILogger<GetUserDetailQueryHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<UserManagementDetailDto>> Handle(
        GetUserDetailQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdminOrStaff)
        {
            throw new AccessDeniedException("Bạn không có quyền xem chi tiết người dùng.");
        }

        var user = await _uow.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("Người dùng không tồn tại.");
        }

        if (_currentUserService.Role == nameof(UserRole.Staff) && user.Role != UserRole.User)
        {
            throw new AccessDeniedException("Bạn không có quyền xem người dùng này.");
        }

        var userSubscription = await _uow.UserSubscriptions.GetActiveSubscriptionAsync(user.Id, cancellationToken);
        CurrentVipPackageDto? currentVipPackage = null;

        if (userSubscription != null)
        {
            var subscription = userSubscription.Subscription;
            var allActiveSubscriptions = await _uow.UserSubscriptions.GetAllActiveByUserIdAsync(user.Id, cancellationToken);
            var sameTypeSubscriptions = allActiveSubscriptions
                .Where(us => us.SubscriptionId == subscription.Id)
                .ToList();

            var earliestStartDate = sameTypeSubscriptions.Min(us => us.StartDate);
            var latestEndDate = sameTypeSubscriptions.Max(us => us.EndDate);

            currentVipPackage = new CurrentVipPackageDto
            {
                SubscriptionId = subscription.Id,
                SubscriptionName = subscription.Name,
                VipLevelName = subscription.LevelOrder.GetDisplayName(),
                AllowedModules = ParseAllowedModuleNames(subscription.AllowedModules),
                StartDate = earliestStartDate,
                EndDate = latestEndDate
            };
        }

        var transactions = await _uow.Transactions.GetByUserIdAsync(user.Id, cancellationToken);
        var transactionDtos = transactions
            .Select(t => new UserTransactionDetailDto
            {
                Id = t.Id,
                OrderCode = t.OrderCode,
                SubscriptionId = t.SubscriptionId,
                SubscriptionName = t.Subscription?.Name ?? string.Empty,
                Amount = t.Amount,
                Status = t.Status,
                StatusName = t.Status.GetDisplayName(),
                Type = t.Type,
                TypeName = t.Type.GetDisplayName(),
                PaymentProvider = t.PaymentProvider,
                PaymentProviderName = t.PaymentProvider.GetDisplayName(),
                ProviderTransactionId = t.ProviderTransactionId,
                CheckoutUrl = t.CheckoutUrl,
                Description = t.Description,
                CreatedAt = t.CreatedAt
            })
            .ToList();

        var result = new UserManagementDetailDto
        {
            Id = user.Id,
            Name = user.Username,
            Role = user.Role.GetDisplayName(),
            Phone = user.PhoneNumber,
            Email = user.Email,
            Avatar = user.AvatarUrl,
            Status = user.Status == CommonStatus.Active ? "Active" : "Inactive",
            CurrentVipPackage = currentVipPackage,
            Transactions = transactionDtos
        };

        _logger.LogInformation("Retrieved detail for user {UserId} by role {Role}", request.UserId, _currentUserService.Role);

        return ApiResponse<UserManagementDetailDto>.Success(result, "Lấy thông tin người dùng thành công.");
    }

    private static List<string> ParseAllowedModuleNames(string allowedModulesJson)
    {
        if (string.IsNullOrWhiteSpace(allowedModulesJson))
        {
            return new List<string>();
        }

        try
        {
            using var document = JsonDocument.Parse(allowedModulesJson);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return new List<string>();
            }

            var moduleNames = new List<string>();

            foreach (var element in document.RootElement.EnumerateArray())
            {
                if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var moduleAsNumber))
                {
                    if (Enum.IsDefined(typeof(ModuleType), moduleAsNumber))
                    {
                        moduleNames.Add(((ModuleType)moduleAsNumber).GetDisplayName());
                    }

                    continue;
                }

                if (element.ValueKind == JsonValueKind.String)
                {
                    var moduleAsString = element.GetString();
                    if (string.IsNullOrWhiteSpace(moduleAsString))
                    {
                        continue;
                    }

                    if (Enum.TryParse<ModuleType>(moduleAsString, true, out var moduleFromName))
                    {
                        moduleNames.Add(moduleFromName.GetDisplayName());
                        continue;
                    }

                    if (int.TryParse(moduleAsString, out var moduleFromStringNumber)
                        && Enum.IsDefined(typeof(ModuleType), moduleFromStringNumber))
                    {
                        moduleNames.Add(((ModuleType)moduleFromStringNumber).GetDisplayName());
                    }
                }
            }

            return moduleNames.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
        catch
        {
            return new List<string>();
        }
    }
}