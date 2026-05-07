namespace GreenDragonTrading.Application.DTOs
{
    /// <summary>
    /// Chat question suggestions returned to the client.
    /// </summary>
    public class ChatSuggestionsDto
    {
        /// <summary>
        /// List of suggested questions in Vietnamese.
        /// </summary>
        public List<string> Questions { get; set; } = new();

        /// <summary>
        /// Symbols from the user's watchlist used as inspiration.
        /// </summary>
        public List<string> SourceSymbols { get; set; } = new();

        /// <summary>
        /// Metadata about the suggestion generation.
        /// </summary>
        public ChatSuggestionsMetadataDto? Metadata { get; set; }
    }

    /// <summary>
    /// Metadata for chat suggestion generation.
    /// </summary>
    public class ChatSuggestionsMetadataDto
    {
        /// <summary>
        /// Name of the AI model used to generate suggestions.
        /// </summary>
        public string ModelName { get; set; } = string.Empty;

        /// <summary>
        /// Processing time in milliseconds.
        /// </summary>
        public long LatencyMs { get; set; }
    }
}
