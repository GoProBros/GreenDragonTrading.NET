using GreenDragonTrading.Application.DTOs;

namespace GreenDragonTrading.Application.Interfaces;

public interface INewsRssService
{
    Task<List<RssNewsItemDto>> FetchCafeFStockNewsAsync(CancellationToken cancellationToken = default);
}
