using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace GreenDragonTrading.Infrastructure.Services;

/// <summary>
/// Service for accessing current authenticated user information from HttpContext and JWT token.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtService _jwtService;
    private readonly Lazy<(Guid? UserId, string? Email)> _userInfo;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        IJwtService jwtService)
    {
        _httpContextAccessor = httpContextAccessor;
        _jwtService = jwtService;
        _userInfo = new Lazy<(Guid? UserId, string? Email)>(ExtractUserInfo);
    }

    /// <inheritdoc/>
    public Guid? UserId => _userInfo.Value.UserId;

    /// <inheritdoc/>
    public string? Email => _userInfo.Value.Email;

    /// <inheritdoc/>
    public bool IsAuthenticated => UserId.HasValue;

    /// <inheritdoc/>
    public string? Role => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

    /// <inheritdoc/>
    public bool IsAdminOrStaff => Role == "Admin" || Role == "Staff";

    /// <inheritdoc/>
    public Guid GetRequiredUserId()
    {
        if (!UserId.HasValue)
        {
            throw new UnauthenticatedException("Người dùng không xác thực.");
        }

        return UserId.Value;
    }

    private (Guid? UserId, string? Email) ExtractUserInfo()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return (null, null);
        }

        var authHeader = httpContext.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return (null, null);
        }

        var token = authHeader["Bearer ".Length..].Trim();
        var tokenInfo = _jwtService.GetTokenInfo(token);

        if (tokenInfo == null)
        {
            return (null, null);
        }

        return (tokenInfo.UserId, tokenInfo.Email);
    }
}
