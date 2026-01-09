using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Ohlcv.Queries.GetOhlcv
{
    /// <summary>
    /// Query để lấy OHLCV data theo ticker và timeframe
    /// Nếu timeframe không có trong DB (M5, M15...), sẽ tự động aggregate từ M1
    /// </summary>
    public class GetOhlcvQuery : IRequest<ApiResponse<OhlcvResponseDto>>
    {
        public string Ticker { get; set; } = null!;

        /// <summary>
        /// Timeframe: M1, M5, M15, M30, H1, H4, D1, W1, MN1
        /// </summary>
        public string Timeframe { get; set; } = "D1";

        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }

        /// <summary>
        /// Số lượng nến tối đa (alternative to time range)
        /// </summary>
        public int? Limit { get; set; }

        /// <summary>
        /// Có sử dụng cache không
        /// </summary>
        public bool UseCache { get; set; } = true;
    }
}