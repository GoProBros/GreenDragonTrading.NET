using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Ohlcv.Queries.QueryOhlcvByFields
{
    /// <summary>
    /// Advanced OHLCV query that supports detailed filtering by individual fields.
    /// </summary>
    public record QueryOhlcvByFieldsQuery : PaginationQuery, IRequest<ApiResponse<PaginatedResponse<OhlcvFieldQueryItemDto>>>
    {
        public string Ticker { get; init; } = string.Empty;
        public string Timeframe { get; init; } = "D1";
        public DateTime? FromTime { get; init; }
        public DateTime? ToTime { get; init; }
        public int? Limit { get; init; }
        public string SortBy { get; init; } = "time";
        public string SortDirection { get; init; } = "desc";
        public List<OhlcvFieldFilterConditionDto> Filters { get; init; } = new();
    }

    public record OhlcvFieldFilterConditionDto
    {
        public string Field { get; init; } = string.Empty;
        public string Operator { get; init; } = "eq";
        public string? Value { get; init; }
    }

    public record OhlcvFieldQueryItemDto
    {
        public DateTime Time { get; init; }
        public string Ticker { get; init; } = string.Empty;
        public string Timeframe { get; init; } = string.Empty;
        public decimal Open { get; init; }
        public decimal High { get; init; }
        public decimal Low { get; init; }
        public decimal Close { get; init; }
        public long Volume { get; init; }
        public decimal? Value { get; init; }
        public int? TradesCount { get; init; }
        public DateTime CreatedAt { get; init; }
        public string Source { get; init; } = string.Empty;
        public bool IsPreliminary { get; init; }
    }
}
