namespace GreenDragonTrading.Application.Interfaces;

/// <summary>
/// Service for accessing current authenticated user information.
/// This provides a clean abstraction over HttpContext and JWT token parsing.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current authenticated user's ID.
    /// Returns null if user is not authenticated.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the current authenticated user's email.
    /// Returns null if user is not authenticated.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Indicates whether the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the current authenticated user's role.
    /// Returns null if user is not authenticated.
    /// </summary>
    string? Role { get; }

    /// <summary>
    /// Indicates whether the current user has Admin or Staff role.
    /// </summary>
    bool IsAdminOrStaff { get; }

    /// <summary>
    /// Gets the current authenticated user's ID.
    /// Throws UnauthenticatedException if user is not authenticated.
    /// </summary>
    /// <returns>The authenticated user's ID</returns>
    /// <exception cref="Domain.Exceptions.UnauthenticatedException">Thrown when user is not authenticated</exception>
    Guid GetRequiredUserId();
}
