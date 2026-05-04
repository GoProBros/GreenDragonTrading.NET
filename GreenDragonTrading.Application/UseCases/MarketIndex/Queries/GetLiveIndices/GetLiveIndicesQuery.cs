using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.MarketIndex.Queries.GetLiveIndices
{
    /// <summary>
    /// Query to retrieve live market index snapshots from Redis for the requested index codes.
    /// </summary>
    public class GetLiveIndicesQuery : IRequest<ApiResponse<List<LiveIndexDataDto>>>
    {
        /// <summary>Index codes to fetch (e.g. ["VNINDEX", "VN30", "HNX30"]).</summary>
        public IReadOnlyList<string> Codes { get; }

        public GetLiveIndicesQuery(IEnumerable<string> codes)
        {
            Codes = codes
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.ToUpperInvariant())
                .Distinct()
                .ToList();
        }
    }
}
