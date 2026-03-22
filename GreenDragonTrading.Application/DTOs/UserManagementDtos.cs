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