using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Application.Common.Models;

namespace GreenDragonTrading.Application.DTOs;

public class PortfolioDto
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public CommonStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string Ticker { get; set; } = string.Empty;

    public decimal AvailableCapital { get; set; }

    public PortfolioSummaryDto Summary { get; set; } = new();
    public PortfolioHistoryPerformanceDto HistoryPerformance { get; set; } = new();
    public PortfolioOverallDto Overall { get; set; } = new();
    public List<PortfolioTransactionHistoryItemDto> TransactionHistory { get; set; } = [];
}

public class PortfolioListItemDto
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public CommonStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string Ticker { get; set; } = string.Empty;

    public decimal TotalInvestedAmount { get; set; }
    public decimal TotalSoldAmount { get; set; }
    public decimal TotalHoldingAmount { get; set; }

    public PortfolioSummaryDto Summary { get; set; } = new();
    public PortfolioHistoryPerformanceDto HistoryPerformance { get; set; } = new();
    public PortfolioOverallDto Overall { get; set; } = new();
    public List<PortfolioTransactionHistoryItemDto> TransactionHistory { get; set; } = [];
}

public class PortfolioListResponseDto
{
    public PaginatedResponse<PortfolioListItemDto> Portfolios { get; set; } = default!;

    public decimal InvestmentCapital { get; set; }
    public decimal TotalInvestedAmount { get; set; }
    public decimal TotalSoldAmount { get; set; }
    public decimal TotalHoldingAmount { get; set; }
}

public class PortfolioSummaryDto
{
    public decimal RemainingQuantity { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal HoldingValue { get; set; }
    public decimal UnrealizedPnL { get; set; }
    public decimal UnrealizedPnLPercent { get; set; }
}

public class PortfolioHistoryPerformanceDto
{
    public decimal TotalBuyQuantity { get; set; }
    public decimal TotalSellQuantity { get; set; }
    public decimal RealizedPnL { get; set; }
    public string? LastTradeDate { get; set; }
}

public class PortfolioOverallDto
{
    public decimal TotalPnL { get; set; }
}

public class PortfolioTransactionHistoryItemDto
{
    public DateTimeOffset TransactionDate { get; set; }
    public TransactionSide Side { get; set; }
    public string SideDisplayName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalValue { get; set; }
    public string? Note { get; set; }
}

public class UserInvestmentCapitalDto
{
    public Guid UserId { get; set; }
    public decimal AvailableCapital { get; set; }
}
