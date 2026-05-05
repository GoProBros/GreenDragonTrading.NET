using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.MarketIndex.Queries.GetIntradayIndex
{
    /// <summary>
    /// Query to retrieve today's intraday price history for a single market index from Redis.
    /// Returns points in chronological order (oldest first) for sparkline rendering.
    /// </summary>
    public class GetIntradayIndexQuery : IRequest<ApiResponse<List<IndexHistoryPointDto>>>
    {
        /// <summary>Index code (e.g. "VNINDEX"). Case-insensitive.</summary>
        public string Code { get; }

        /// <summary>Maximum number of history points to return. Pass -1 (default) to return all points for the day.</summary>
        public int MaxPoints { get; }

        public GetIntradayIndexQuery(string code, int maxPoints = -1)
        {
            Code = code.ToUpperInvariant();
            // -1 (or 0) means "all"; positive values cap the result.
            MaxPoints = maxPoints <= 0 ? -1 : maxPoints;
        }
    }
}
