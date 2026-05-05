namespace GreenDragonTrading.Application.Common.Options
{
    public class AiEngineOptions
    {
        public const string SectionName = "AiEngine";
        public string BaseUrl { get; set; } = null!;
        public int TimeoutSeconds { get; set; }
        /// <summary>
        /// API key used to authenticate requests to the AI engine.
        /// </summary>
        public string? ApiKey { get; set; }
        public string ChatEndpoint { get; set; } = "/api/chat";
        public string ConversationSummaryEndpoint { get; set; } = "/api/conv-summary";
        public string NewsSummarizationEndpoint { get; set; } = "/api/summarization";
        public string ProactiveAlertEvaluationEndpoint { get; set; } = "/api/proactive-alerts/evaluate";
        public string SuggestionsEndpoint { get; set; } = "/api/chat/suggestions";
        /// <summary>
        /// Number of recent messages (user + AI combined) to include as context in each request.
        /// </summary>
        public int RecentMessagesLimit { get; set; } = 20;
        /// <summary>
        /// Number of messages to summarize per batch when threshold is reached.
        /// </summary>
        public int SummaryBatchSize { get; set; } = 15;
        /// <summary>
        /// Number of newest messages to keep unsummarized when creating a summary batch.
        /// </summary>
        public int SummaryKeepRecentCount { get; set; } = 5;
        public int ChatAsyncPollingIntervalSeconds { get; set; } = 2;
        public int ChatAsyncJobTtlMinutes { get; set; } = 30;
    }
}
