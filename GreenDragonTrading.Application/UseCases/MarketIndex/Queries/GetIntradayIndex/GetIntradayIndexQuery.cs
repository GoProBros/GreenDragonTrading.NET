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

        /// <summary>Maximum number of history points to return (default 4000 — covers full trading day at 5s intervals).</summary>
        public int MaxPoints { get; }

        public GetIntradayIndexQuery(string code, int maxPoints = 4000)
        {
            Code = code.ToUpperInvariant();
            MaxPoints = Math.Clamp(maxPoints, 1, 4000);
        }
    }
}
