using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Indicators.Queries.GetIndicatorSnapshotZScore
{
    /// <summary>
    /// Query to get z-score normalized indicator snapshot from Redis for a ticker/timeframe.
    /// </summary>
    public class GetIndicatorSnapshotZScoreQuery : IRequest<ApiResponse<IndicatorSnapshotZScoreDto>>
    {
        public string Ticker { get; }
        public string Timeframe { get; }

        public GetIndicatorSnapshotZScoreQuery(string ticker, string timeframe = "D1")
        {
            Ticker = ticker.ToUpperInvariant();
            Timeframe = timeframe.ToUpperInvariant();
        }
    }
}
