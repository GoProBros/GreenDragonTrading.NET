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
            int statusCode = StatusCodes.Status500InternalServerError;

            switch (exception)
            {
                case DomainException domainEx when
                    domainEx is ValidationException || domainEx is BusinessRuleException:
                    statusCode = StatusCodes.Status400BadRequest;
                    response = ApiResponse.FailResponse("Lỗi kiểm tra dữ liệu đầu vào.", domainEx.Message);
                    break;

                case NotFoundException domainEx:
                    statusCode = StatusCodes.Status404NotFound;
                    response = ApiResponse.FailResponse("Không tìm thấy tài nguyên.", domainEx.Message);
                    break;

                case ConflictException domainEx:
                    statusCode = StatusCodes.Status409Conflict;
                    response = ApiResponse.FailResponse("Tài nguyên đã tồn tại.", domainEx.Message);
                    break;

                case AccessDeniedException domainEx:
                    statusCode = StatusCodes.Status403Forbidden;
                    response = ApiResponse.FailResponse("Bạn không có quyền thực hiện hành động này.", domainEx.Message);
                    break;

                case UnauthenticatedException domainEx:
                    statusCode = StatusCodes.Status401Unauthorized;
                    response = ApiResponse.FailResponse("Yêu cầu cần đăng nhập.");
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    response = ApiResponse.FailResponse("Lỗi hệ thống.");
                    break;
            }

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            return true;
        }
    }
}
