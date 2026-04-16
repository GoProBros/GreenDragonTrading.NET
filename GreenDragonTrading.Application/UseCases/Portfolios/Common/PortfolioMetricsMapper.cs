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
        decimal totalBuyQuantity = 0m;
        decimal totalSellQuantity = 0m;
        decimal remainingQuantity = 0m;
        decimal averagePrice = 0m;
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

                var currentCost = remainingQuantity * averagePrice;
                var buyCost = quantity * price;

                remainingQuantity += quantity;
                averagePrice = remainingQuantity > 0m
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

                realizedPnL += (price - averagePrice) * sellableQuantity;
                remainingQuantity -= sellableQuantity;

                if (remainingQuantity == 0m)
                {
                    averagePrice = 0m;
                }
            }
        }

        var holdingValue = remainingQuantity > 0m
            ? remainingQuantity * currentPrice
            : 0m;

        var investedCapital = remainingQuantity > 0m
            ? averagePrice * remainingQuantity
            : 0m;

        var unrealizedPnL = holdingValue - investedCapital;
        var unrealizedPnLPercent = investedCapital > 0m
            ? (unrealizedPnL / investedCapital) * 100m
            : 0m;

        var totalPnL = realizedPnL + unrealizedPnL;

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
                RemainingQuantity = remainingQuantity,
                AveragePrice = averagePrice,
                CurrentPrice = currentPrice,
                HoldingValue = holdingValue,
                UnrealizedPnL = unrealizedPnL,
                UnrealizedPnLPercent = unrealizedPnLPercent
            },
            HistoryPerformance = new PortfolioHistoryPerformanceDto
            {
                TotalBuyQuantity = totalBuyQuantity,
                TotalSellQuantity = totalSellQuantity,
                RealizedPnL = realizedPnL,
                LastTradeDate = lastTradeDate?.ToString("yyyy-MM-dd")
            },
            Overall = new PortfolioOverallDto
            {
                TotalPnL = totalPnL
            },
            TransactionHistory = transactionHistory
        };
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