using System.Text.Json.Serialization;

namespace GreenDragonTrading.Application.Common.Models
{
    public record ApiResponse
    {
        public bool IsSuccess { get; init; }
        public string Message { get; init; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Errors { get; init; }

        public DateTime ResponseTime { get; init; } = DateTime.Now;

        protected ApiResponse(bool isSuccess, string message, List<string>? errors = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            Errors = errors;
        }

        #region Factory Methods (Non-Generic)

        public static ApiResponse Success(string message = "Thành công")
            => new(true, message);

        public static ApiResponse Failure(string message, List<string> errors) => new(false, message, errors);

        public static ApiResponse Failure(string message, string? error = null)
        {
            List<string>? errorList = !string.IsNullOrWhiteSpace(error) ? [error] : null;

            return new(false, message, errorList);
        }

        #endregion
    }

    public record ApiResponse<T> : ApiResponse
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Data { get; init; }

        private ApiResponse(bool isSuccess, string message, T? data, List<string>? errors = null)
            : base(isSuccess, message, errors)
        {
            Data = data;
        }

        #region Factory Methods (Generic)

        public static ApiResponse<T> Success(T data, string message = "Thành công")
            => new(true, message, data);

        public new static ApiResponse<T> Failure(string message, List<string> errors)
            => new(false, message, default, errors);

        public new static ApiResponse<T> Failure(string message, string? error = null)
        {
            List<string>? errorList = !string.IsNullOrWhiteSpace(error) ? [error] : null;
            return new(false, message, default, errorList);
        }

        #endregion
    }
}