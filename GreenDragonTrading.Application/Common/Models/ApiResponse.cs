using System.Text.Json.Serialization;

namespace GreenDragonTrading.Application.Common.Models
{
    public record ApiResponse
    {
        public bool IsSuccess { get; init; }
        public string Message { get; init; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, string[]>? ValidationErrors { get; init; }

        public DateTime ResponseTime { get; init; } = DateTime.Now;

        protected ApiResponse(
            bool isSuccess,
            string message,
            IDictionary<string, string[]>? validationErrors = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            ValidationErrors = validationErrors;
        }

        #region Factory Methods (Non-Generic)

        public static ApiResponse Success(string message = "Thành công")
            => new(true, message);

        public static ApiResponse Failure(
            string message,
            IDictionary<string, string[]>? validationErrors = null)
            => new(false, message, validationErrors);

        public static ApiResponse Failure(string message)
            => new(false, message);

        #endregion
    }

    public record ApiResponse<T> : ApiResponse
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Data { get; init; }

        private ApiResponse(
            bool isSuccess,
            string message,
            T? data,
            IDictionary<string, string[]>? validationErrors = null)
            : base(isSuccess, message, validationErrors)
        {
            Data = data;
        }

        #region Factory Methods (Generic)

        public static ApiResponse<T> Success(T data, string message = "Thành công")
            => new(true, message, data);

        public new static ApiResponse<T> Failure(
            string message,
            IDictionary<string, string[]>? validationErrors = null)
            => new(false, message, default, validationErrors);

        public new static ApiResponse<T> Failure(string message)
            => new(false, message, default);
        #endregion
    }
}