using GreenDragonTrading.Application.DTOs;

namespace GreenDragonTrading.Application.Interfaces
{
    public interface ISsiService
    {
        Task<List<SsiSymbolDto>> FetchSymbolsListAsync(string exchange, CancellationToken cancellationToken = default);

        Task<SsiSymbolDetailsDto?> FetchSymbolsDetailsAsync(string symbol, CancellationToken cancellationToken = default);

        Task<List<SsiIndustryDto>> FetchIndustryListAsync(CancellationToken cancellationToken = default);
    }
}
