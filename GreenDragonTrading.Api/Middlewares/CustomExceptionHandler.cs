using FluentValidation;
using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace GreenDragonTrading.Api.Middlewares
{
    public class CustomExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            httpContext.Response.ContentType = "application/json";
            ApiResponse response;
            int statusCode;

            switch (exception)
            {
                case BusinessRuleException businessEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    response = ApiResponse.Failure(businessEx.Message);
                    break;

                case Domain.Exceptions.ValidationException domainValidationEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    response = ApiResponse.Failure("Lỗi kiểm tra dữ liệu đầu vào.", domainValidationEx.Errors);
                    break;

                case NotFoundException notFoundEx: 
                    statusCode = StatusCodes.Status404NotFound;
                    response = ApiResponse.Failure(notFoundEx.Message);
                    break;

                case ConflictException conflictEx: 
                    statusCode = StatusCodes.Status409Conflict;
                    response = ApiResponse.Failure(conflictEx.Message);
                    break;

                case AccessDeniedException accessDeniedEx: 
                    statusCode = StatusCodes.Status403Forbidden;
                    response = ApiResponse.Failure(accessDeniedEx.Message);
                    break;

                case UnauthenticatedException unauthenticatedEx: 
                    statusCode = StatusCodes.Status401Unauthorized;
                    response = ApiResponse.Failure(unauthenticatedEx.Message);
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    response = ApiResponse.Failure("Đã xảy ra lỗi máy chủ không mong muốn.");
                    break;
            }

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            return true;
        }
    }
}
