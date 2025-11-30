using System.Text.Json.Serialization;

namespace GreenDragonTrading.Application.DTOs
{
    public class VndPaginatedApiResponse<T>
    {
        [JsonPropertyName("data")]
        public List<T>? Data { get; set; }

        [JsonPropertyName("totalElements")]
        public int? TotalElements { get; set; }

        [JsonPropertyName("size")]
        public int? PageSize { get; set; }

        [JsonPropertyName("currentPage")]
        public int? CurrentPage { get; set; }

        [JsonPropertyName("totalPages")]
        public int? TotalPages { get; set; }
    }

    public class VndIndustryResponseDto
    {
        [JsonPropertyName("industryCode")]
        public string? IndustryCode { get; set; }

        [JsonPropertyName("industryLevel")]
        public string? IndustryLevel { get; set; }

        [JsonPropertyName("higherLevelCode")]
        public string? HigherLevelCode { get; set; }

        [JsonPropertyName("englishName")]
        public string? EnglishName { get; set; }

        [JsonPropertyName("vietnameseName")]
        public string? VietnameseName { get; set; }

        [JsonPropertyName("totalCount")]
        public int? TotalCount { get; set; }
    }
}
