using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Application.DTOs
{
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
