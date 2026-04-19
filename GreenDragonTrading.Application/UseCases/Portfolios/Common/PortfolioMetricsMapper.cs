using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace GreenDragonTrading.Application.UseCases.Portfolios.Common;

internal static class PortfolioMetricsMapper
{
    public static PortfolioDto ToDto(
        Portfolio portfolio,
        IEnumerable<TradingTransaction> transactions,
        decimal availableCapital,
        decimal currentPrice)
    {
        var metrics = CalculateMetrics(transactions, currentPrice);

        return new PortfolioDto
        {
            Id = portfolio.Id,
            UserId = portfolio.UserId,
            Name = portfolio.Name,
            Description = portfolio.Description,
            Status = portfolio.Status,
            CreatedAt = portfolio.CreatedAt,
            Ticker = NormalizeTicker(portfolio.Ticker),
            AvailableCapital = availableCapital,
            Summary = new PortfolioSummaryDto
            {
                RemainingQuantity = metrics.RemainingQuantity,
                AveragePrice = metrics.AveragePrice,
                CurrentPrice = currentPrice,
                HoldingValue = metrics.HoldingValue,
                UnrealizedPnL = metrics.UnrealizedPnL,
                UnrealizedPnLPercent = metrics.UnrealizedPnLPercent
            },
            HistoryPerformance = new PortfolioHistoryPerformanceDto
            {
                TotalBuyQuantity = metrics.TotalBuyQuantity,
                TotalSellQuantity = metrics.TotalSellQuantity,
                RealizedPnL = metrics.RealizedPnL,
                LastTradeDate = metrics.LastTradeDate?.ToString("yyyy-MM-dd")
            },
            Overall = new PortfolioOverallDto
            {
                TotalPnL = metrics.TotalPnL
            },
            TransactionHistory = metrics.TransactionHistory
        };
    }

    public static PortfolioListItemDto ToListItemDto(
        Portfolio portfolio,
        IEnumerable<TradingTransaction> transactions,
        decimal currentPrice)
    {
        var metrics = CalculateMetrics(transactions, currentPrice);

        return new PortfolioListItemDto
        {
            Id = portfolio.Id,
            UserId = portfolio.UserId,
            Name = portfolio.Name,
            Description = portfolio.Description,
            Status = portfolio.Status,
            CreatedAt = portfolio.CreatedAt,
            Ticker = NormalizeTicker(portfolio.Ticker),
            TotalInvestedAmount = metrics.TotalInvestedAmount,
            TotalSoldAmount = metrics.TotalSoldAmount,
            TotalHoldingAmount = metrics.TotalHoldingAmount,
            Summary = new PortfolioSummaryDto
            {
                RemainingQuantity = metrics.RemainingQuantity,
                AveragePrice = metrics.AveragePrice,
                CurrentPrice = currentPrice,
                HoldingValue = metrics.HoldingValue,
                UnrealizedPnL = metrics.UnrealizedPnL,
                UnrealizedPnLPercent = metrics.UnrealizedPnLPercent
            },
            HistoryPerformance = new PortfolioHistoryPerformanceDto
            {
                TotalBuyQuantity = metrics.TotalBuyQuantity,
                TotalSellQuantity = metrics.TotalSellQuantity,
                RealizedPnL = metrics.RealizedPnL,
                LastTradeDate = metrics.LastTradeDate?.ToString("yyyy-MM-dd")
            },
            Overall = new PortfolioOverallDto
            {
                TotalPnL = metrics.TotalPnL
            },
            TransactionHistory = metrics.TransactionHistory
        };
    }

    private static PortfolioCalculatedMetrics CalculateMetrics(
        IEnumerable<TradingTransaction> transactions,
        decimal currentPrice)
    {
        decimal totalBuyQuantity = 0m;
        decimal totalSellQuantity = 0m;
        decimal totalBuyAmount = 0m;
        decimal totalSoldAmount = 0m;
        decimal remainingQuantity = 0m;
        decimal remainingAveragePrice = 0m;
        decimal realizedPnL = 0m;
        DateTimeOffset? lastTradeDate = null;
        var transactionHistory = new List<PortfolioTransactionHistoryItemDto>();

        foreach (var transaction in transactions
                     .OrderBy(x => x.TransactionDate)
                     .ThenBy(x => x.RecordedAt)
                     .ThenBy(x => x.Id))
        {
            var quantity = transaction.Quantity ?? 0m;
            var price = transaction.Price ?? 0m;

            if (quantity <= 0m)
            {
                continue;
            }

            lastTradeDate = transaction.TransactionDate;

            transactionHistory.Add(new PortfolioTransactionHistoryItemDto
            {
                TransactionDate = transaction.TransactionDate,
                Side = transaction.Side,
                SideDisplayName = GetSideDisplayName(transaction.Side),
                Quantity = quantity,
                Price = price,
                TotalValue = quantity * price,
                Note = transaction.Note
            });

            if (transaction.Side == TransactionSide.Buy)
            {
                totalBuyQuantity += quantity;

                var currentCost = remainingQuantity * remainingAveragePrice;
                var buyCost = quantity * price;
                totalBuyAmount += buyCost;

                remainingQuantity += quantity;
                remainingAveragePrice = remainingQuantity > 0m
                    ? (currentCost + buyCost) / remainingQuantity
                    : 0m;

                continue;
            }

            if (transaction.Side == TransactionSide.Sell)
            {
                totalSellQuantity += quantity;

                var sellableQuantity = Math.Min(quantity, remainingQuantity);
                if (sellableQuantity <= 0m)
                {
                    continue;
                }

                realizedPnL += (price - remainingAveragePrice) * sellableQuantity;
                totalSoldAmount += sellableQuantity * price;
                remainingQuantity -= sellableQuantity;

                if (remainingQuantity == 0m)
                {
                    remainingAveragePrice = 0m;
                }
            }
        }

        var averageBuyPrice = totalBuyQuantity > 0m
            ? totalBuyAmount / totalBuyQuantity
            : 0m;

        var holdingValue = remainingQuantity > 0m
            ? remainingQuantity * currentPrice
            : 0m;

        var investedCapital = remainingQuantity > 0m
            ? remainingAveragePrice * remainingQuantity
            : 0m;

        var unrealizedPnL = holdingValue - investedCapital;
        var unrealizedPnLPercent = investedCapital > 0m
            ? (unrealizedPnL / investedCapital) * 100m
            : 0m;

        return new PortfolioCalculatedMetrics
        {
            TotalBuyQuantity = totalBuyQuantity,
            TotalSellQuantity = totalSellQuantity,
            TotalBuyAmount = totalBuyAmount,
            TotalSoldAmount = totalSoldAmount,
            RemainingQuantity = remainingQuantity,
            AveragePrice = averageBuyPrice,
            RealizedPnL = realizedPnL,
            LastTradeDate = lastTradeDate,
            HoldingValue = holdingValue,
            UnrealizedPnL = unrealizedPnL,
            UnrealizedPnLPercent = unrealizedPnLPercent,
            TotalPnL = realizedPnL + unrealizedPnL,
            TransactionHistory = transactionHistory
        };
    }

    private sealed class PortfolioCalculatedMetrics
    {
        public decimal TotalBuyQuantity { get; init; }
        public decimal TotalSellQuantity { get; init; }
        public decimal TotalBuyAmount { get; init; }
        public decimal TotalSoldAmount { get; init; }
        public decimal RemainingQuantity { get; init; }
        public decimal AveragePrice { get; init; }
        public decimal RealizedPnL { get; init; }
        public DateTimeOffset? LastTradeDate { get; init; }
        public decimal HoldingValue { get; init; }
        public decimal UnrealizedPnL { get; init; }
        public decimal UnrealizedPnLPercent { get; init; }
        public decimal TotalPnL { get; init; }
        public List<PortfolioTransactionHistoryItemDto> TransactionHistory { get; init; } = [];

        public decimal TotalInvestedAmount => TotalBuyAmount;
        public decimal TotalHoldingAmount => HoldingValue;
    }

    private static string GetSideDisplayName(TransactionSide side)
    {
        var member = typeof(TransactionSide).GetMember(side.ToString()).FirstOrDefault();
        if (member == null)
        {
            return side.ToString();
        }

        var display = member.GetCustomAttribute<DisplayAttribute>();
        return string.IsNullOrWhiteSpace(display?.Name)
            ? side.ToString()
            : display!.Name!;
    }

    private static string NormalizeTicker(string? ticker)
    {
        return string.IsNullOrWhiteSpace(ticker)
            ? string.Empty
            : ticker.Trim().ToUpperInvariant();
    }
}