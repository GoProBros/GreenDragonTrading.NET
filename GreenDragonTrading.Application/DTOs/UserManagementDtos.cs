using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Application.DTOs;

/// <summary>
/// User information item for management list.
/// </summary>
public class UserManagementListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// User detail information for management page.
/// </summary>
public class UserManagementDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string Status { get; set; } = string.Empty;
    public CurrentVipPackageDto? CurrentVipPackage { get; set; }
    public List<UserTransactionDetailDto> Transactions { get; set; } = new();
}

/// <summary>
/// Current VIP package information of a user.
/// </summary>
public class CurrentVipPackageDto
{
    public int SubscriptionId { get; set; }
    public string SubscriptionName { get; set; } = string.Empty;
    public string VipLevelName { get; set; } = string.Empty;
    public List<string> AllowedModules { get; set; } = new();
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
}

/// <summary>
/// Transaction detail item for user management detail.
/// </summary>
public class UserTransactionDetailDto
{
    public Guid Id { get; set; }
    public long OrderCode { get; set; }
    public int SubscriptionId { get; set; }
    public string SubscriptionName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TransactionStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public PaymentType PaymentProvider { get; set; }
    public string PaymentProviderName { get; set; } = string.Empty;
    public string? ProviderTransactionId { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>
/// Request to create a staff account.
/// </summary>
public class CreateStaffUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public bool RequireEmailVerification { get; set; } = true;
}

/// <summary>
/// Request to update user status (ban/unban).
/// </summary>
public class UpdateUserStatusRequest
{
    public CommonStatus Status { get; set; }
}