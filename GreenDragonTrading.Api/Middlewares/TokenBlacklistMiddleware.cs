using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace GreenDragonTrading.Api.Middlewares
{
    /// <summary>
    /// Middleware to check if access token is blacklisted
    /// </summary>
    public class TokenBlacklistMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenBlacklistMiddleware> _logger;

        public TokenBlacklistMiddleware(
            RequestDelegate next,
            ILogger<TokenBlacklistMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ITokenBlacklistService tokenBlacklistService)
        {
            // Bỏ qua nếu không có token
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            try
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                var handler = new JwtSecurityTokenHandler();
                
                if (handler.CanReadToken(token))
                {
                    var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
                    var jti = jsonToken?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

                    if (!string.IsNullOrEmpty(jti))
                    {
                        var isBlacklisted = await tokenBlacklistService.IsTokenBlacklistedAsync(jti);
                        
                        if (isBlacklisted)
                        {
                            _logger.LogWarning("Token đã bị blacklist {Jti} cố gắng truy cập", jti);
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";
                            
                            var response = ApiResponse.Failure("Token đã bị thu hồi. Vui lòng đăng nhập lại.");
                            await context.Response.WriteAsJsonAsync(response);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra token blacklist");
            }

            await _next(context);
        }
    }

    /// <summary>
    /// Extension method to register middleware
    /// </summary>
    public static class TokenBlacklistMiddlewareExtensions
    {
        public static IApplicationBuilder UseTokenBlacklist(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenBlacklistMiddleware>();
        }
    }
}
