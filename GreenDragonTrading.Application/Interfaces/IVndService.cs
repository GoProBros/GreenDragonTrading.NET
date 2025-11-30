using GreenDragonTrading.Application.DTOs;

namespace GreenDragonTrading.Application.Interfaces
{
    public interface IVndService
    {
        Task<List<VndIndustryResponseDto>> FetchIndustriesListAsync(string level, string pageSize, CancellationToken cancellationToken = default);
    }
}
