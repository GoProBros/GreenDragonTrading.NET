namespace GreenDragonTrading.Application.Common.Options
{
    public class AiEngineOptions
    {
        public const string SectionName = "AiEngine";
        public string BaseUrl { get; set; } = null!;
        public int TimeoutSeconds { get; set; }
        /// <summary>
        /// Number of recent messages (user + AI combined) to include as context in each request.
        /// </summary>
        public int RecentMessagesLimit { get; set; } = 20;
    }
}
