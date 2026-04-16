namespace GreenDragonTrading.Application.DTOs;

/// <summary>
/// Customer retention statistics based on package registration frequency.
/// </summary>
public class CustomerRetentionStatisticsDto
{
    public int TotalActiveCustomers { get; set; }
    public int CustomersRegisteredAtLeast1Time { get; set; }
    public int CustomersRegisteredAtLeast3Times { get; set; }
    public int CustomersRegisteredAtLeast6Times { get; set; }
    public decimal CustomersRegisteredAtLeast1TimeRate { get; set; }
    public decimal CustomersRegisteredAtLeast3TimesRate { get; set; }
    public decimal CustomersRegisteredAtLeast6TimesRate { get; set; }
}

/// <summary>
/// Top interested symbols statistics aggregated from active users' watch lists.
/// </summary>
public class WatchListTopInterestedSymbolsDto
{
    public int ActiveCustomerCount { get; set; }
    public int ProcessedWatchListCount { get; set; }
    public int TotalTickerOccurrences { get; set; }
    public List<InterestedSymbolCountDto> Top5Symbols { get; set; } = new();
    public List<InterestedSymbolCountDto> Top10Symbols { get; set; } = new();
}

/// <summary>
/// Symbol occurrence count in watch lists.
/// </summary>
public class InterestedSymbolCountDto
{
    public string Symbol { get; set; } = string.Empty;
    public int Count { get; set; }
}
