namespace GreenDragonTrading.Application.Common.Options
{
    public class AiEngineOptions
    {
        public const string SectionName = "AiEngine";
        public string BaseUrl { get; set; } = null!;
        public int TimeoutSeconds { get; set; }
        public string ChatEndpoint { get; set; } = "/api/chat";
        public string ConversationSummaryEndpoint { get; set; } = "/api/conv-summary";
        public string NewsSummarizationEndpoint { get; set; } = "/api/summarization";
        public string ProactiveAlertEvaluationEndpoint { get; set; } = "/api/proactive-alerts/evaluate";
        /// <summary>
        /// Number of recent messages (user + AI combined) to include as context in each request.
        /// </summary>
        public int RecentMessagesLimit { get; set; } = 20;
        public int ChatAsyncPollingIntervalSeconds { get; set; } = 2;
        public int ChatAsyncJobTtlMinutes { get; set; } = 30;
    }
}
