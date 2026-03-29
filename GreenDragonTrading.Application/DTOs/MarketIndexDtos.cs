using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Application.DTOs
{
    /// <summary>
    /// Real-time snapshot of a market index value fetched from Redis or broadcast via SignalR.
    /// Mirrors the frontend LiveIndexData TypeScript interface.
    /// </summary>
    public class LiveIndexDataDto
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public double IndexValue { get; set; }
        public double Change { get; set; }
        public double RatioChange { get; set; }
        public double RefIndex { get; set; }
        public double OpenIndex { get; set; }
        public double HighIndex { get; set; }
        public double LowIndex { get; set; }
        public double TotalTrade { get; set; }
        public double TotalMatchVol { get; set; }
        public double TotalMatchVal { get; set; }
        public int AdvanceCount { get; set; }
        public int DeclineCount { get; set; }
        public int NoChangeCount { get; set; }
        /// <summary>Number of stocks hitting ceiling price within this index basket (from SSI MI "Ceilings" field).</summary>
        public int CeilingCount { get; set; }
        /// <summary>Number of stocks hitting floor price within this index basket (from SSI MI "Floors" field).</summary>
        public int FloorCount { get; set; }
        public string? Exchange { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// One data point in an index's intraday price history (stored as Redis list).
    /// Mirrors the frontend IndexHistoryPoint TypeScript interface.
    /// </summary>
    public class IndexHistoryPointDto
    {
        public string Time { get; set; } = null!;
        public double Value { get; set; }
    }

    /// <summary>
    /// Summary DTO for a market index (used in list responses).
    /// </summary>
    public class MarketIndexDto
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string ExchangeCode { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsBenchmark { get; set; }
        public CommonStatus Status { get; set; }
    }

    /// <summary>
    /// Constituent symbol entry inside an index.
    /// </summary>
    public class MarketIndexSymbolDto
    {
        public string Ticker { get; set; } = null!;
        public string? EnCompanyName { get; set; }
        public string? ViCompanyName { get; set; }
        public string? ExchangeCode { get; set; }
        public decimal? Weight { get; set; }
        public DateOnly? AddedDate { get; set; }
        public int DisplayOrder { get; set; }
    }
}
