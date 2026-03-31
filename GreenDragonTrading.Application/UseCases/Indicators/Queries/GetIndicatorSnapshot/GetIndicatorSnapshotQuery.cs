using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Indicators.Queries.GetIndicatorSnapshot
{
    /// <summary>
    /// Query to get the latest raw indicator snapshot from Redis for a ticker/timeframe.
    /// </summary>
    public class GetIndicatorSnapshotQuery : IRequest<ApiResponse<IndicatorSnapshotDto>>
    {
        public string Ticker { get; }
        public string Timeframe { get; }

        public GetIndicatorSnapshotQuery(string ticker, string timeframe = "D1")
        {
            Ticker = ticker.ToUpperInvariant();
            Timeframe = timeframe.ToUpperInvariant();
        }
    }
}
