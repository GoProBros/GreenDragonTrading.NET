namespace GreenDragonTrading.Application.DTOs
{
    /// <summary>
    /// Client result wrapper for proactive evaluation endpoint calls.
    /// </summary>
    public class ProactiveAlertEvaluationClientResult
    {
        public bool Success { get; init; }
        public bool IsValidationError { get; init; }
        public bool IsConflict { get; init; }
        public string? ErrorCode { get; init; }
        public string? ErrorMessage { get; init; }
        public ProactiveAlertEvaluationResponse? Data { get; init; }

        public static ProactiveAlertEvaluationClientResult Failure(
            string? errorMessage,
            string? errorCode = null,
            bool isValidationError = false,
            bool isConflict = false)
            => new()
            {
                Success = false,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode,
                IsValidationError = isValidationError,
                IsConflict = isConflict,
            };
    }

    /// <summary>
    /// Successful response payload returned by proactive evaluation endpoint.
    /// </summary>
    public class ProactiveAlertEvaluationResponse
    {
        public string JobId { get; set; } = string.Empty;
        public string Ticker { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public decimal Confidence { get; set; }
        public List<string> Rationale { get; set; } = new();
        public string SuggestedAction { get; set; } = string.Empty;
        public int TtlMinutes { get; set; }
        public string UserMessage { get; set; } = string.Empty;
        public ProactiveAlertEvaluationMetadata? Metadata { get; set; }
    }

    /// <summary>
    /// Metadata returned by proactive evaluation endpoint.
    /// </summary>
    public class ProactiveAlertEvaluationMetadata
    {
        public string? ModelName { get; set; }
        public string? PromptVersion { get; set; }
        public int? LatencyMs { get; set; }
        public bool UsedFallback { get; set; }
    }
}
