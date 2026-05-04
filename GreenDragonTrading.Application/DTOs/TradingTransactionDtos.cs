using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Application.DTOs;

public class TradingTransactionDto
{
    public int Id { get; set; }
    public int PortfolioId { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public TransactionSide Side { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? Price { get; set; }
    public DateTimeOffset TransactionDate { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
    public string? Note { get; set; }
    public string? OriginalMessage { get; set; }
}
