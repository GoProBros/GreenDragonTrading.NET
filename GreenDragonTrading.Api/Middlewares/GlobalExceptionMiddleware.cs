using System.Net;
using System.Text.Json;
using FluentValidation;
using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Domain.Exceptions;

namespace GreenDragonTrading.Api.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An error occurred while processing your request";
        List<string>? errors = null;

        switch (exception)
        {
            case ValidationException validationException:
                _logger.LogWarning("Validation failed: {Errors}", 
                    string.Join(", ", validationException.Errors.Select(e => e.ErrorMessage)));
                statusCode = HttpStatusCode.BadRequest;
                message = "Validation failed";
                errors = validationException.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                break;

            case UnauthorizedException:
                _logger.LogWarning("Unauthorized access: {Message}", exception.Message);
                statusCode = HttpStatusCode.Unauthorized;
                message = exception.Message;
                break;

            case NotFoundException notFoundException:
                _logger.LogWarning("Resource not found: {EntityName} {Key}", 
                    notFoundException.EntityName, notFoundException.Key);
                statusCode = HttpStatusCode.NotFound;
                message = exception.Message;
                break;

            case ConflictException conflictException:
                _logger.LogWarning("Conflict: {Message}", exception.Message);
                statusCode = HttpStatusCode.Conflict;
                message = exception.Message;
                break;

            case BusinessRuleException businessException:
                _logger.LogWarning("Business rule violation [{Code}]: {Message}", 
                    businessException.Code, exception.Message);
                statusCode = HttpStatusCode.BadRequest;
                message = exception.Message;
                break;

            case DomainException:
                _logger.LogWarning("Domain exception: {Message}", exception.Message);
                statusCode = HttpStatusCode.BadRequest;
                message = exception.Message;
                break;

            default:
                // Only log as Error for unexpected exceptions
                _logger.LogError(exception, "An unexpected error occurred: {Message}", exception.Message);
                break;
        }

        response.StatusCode = (int)statusCode;

        var result = ApiResponse.FailResponse(message, errors ?? new List<string> { exception.Message });

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await response.WriteAsync(JsonSerializer.Serialize(result, options));
    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
