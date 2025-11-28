namespace GreenDragonTrading.Domain.Enums;

/// <summary>
/// Represents the role of a user in the system.
/// </summary>
public enum UserRole : short
{
    /// <summary>
    /// Regular user with limited permissions.
    /// </summary>
    User = 0,

    /// <summary>
    /// Administrator with full access.
    /// </summary>
    Admin = 1,

    /// <summary>
    /// Moderator with partial administrative permissions.
    /// </summary>
    Moderator = 2
}
