using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Application.DTOs
{
    public class SymbolDtos
    {
        public string Ticker { get; set; } = null!;

        public string? Isin { get; set; }

        public string? EnCompanyName { get; set; } = null!;

        public string? ViCompanyName { get; set; }

        public string ExchangeCode { get; set; } = null!;

        public string? SectorId { get; set; }

        public SymbolType Type { get; set; }

        public CommonStatus Status { get; set; } = CommonStatus.Active;
    }
}
