using GreenDragonTrading.Domain.Enums;
using System.Text.Json;

namespace GreenDragonTrading.Application.DTOs;

public class WatchListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public JsonElement Tickers { get; set; }
    public CommonStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class WatchListListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TickerCount { get; set; }
    public CommonStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
