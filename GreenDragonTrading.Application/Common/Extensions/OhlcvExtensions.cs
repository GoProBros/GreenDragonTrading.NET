using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Application.Common.Extensions
{
    /// <summary>
    /// Extension methods cho OHLCV operations
    /// </summary>
    public static class OhlcvExtensions
    {
        /// <summary>
        /// Convert Entity sang DTO
        /// </summary>
        public static OhlcvDto ToDto(this Ohlcv entity)
        {
            return new OhlcvDto
            {
                Time = new DateTimeOffset(entity.Time).ToUnixTimeMilliseconds(),
                Open = entity.Open,
                High = entity.High,
                Low = entity.Low,
                Close = entity.Close,
                Volume = entity.Volume,
                Value = entity.Value
            };
        }

        /// <summary>
        /// Convert list entities sang list DTOs
        /// </summary>
        public static List<OhlcvDto> ToDtoList(this IEnumerable<Ohlcv> entities)
        {
            return entities.Select(e => e.ToDto()).ToList();
        }

        /// <summary>
        /// Convert DTO sang Entity
        /// </summary>
        public static Ohlcv ToEntity(this OhlcvDto dto, string ticker, string timeframe, string source = "API")
        {
            return new Ohlcv
            {
                Time = DateTimeOffset.FromUnixTimeMilliseconds(dto.Time).UtcDateTime,
                Ticker = ticker,
                Timeframe = timeframe,
                Open = dto.Open,
                High = dto.High,
                Low = dto.Low,
                Close = dto.Close,
                Volume = dto.Volume,
                Value = dto.Value,
                CreatedAt = DateTime.UtcNow,
                Source = source
            };
        }

        /// <summary>
        /// Check if candle is complete (không còn update nữa)
        /// </summary>
        public static bool IsComplete(this Ohlcv candle, string timeframe)
        {
            var now = DateTime.UtcNow;
            
            return timeframe switch
            {
                "M1" => candle.Time.AddMinutes(1) < now,
                "M5" => candle.Time.AddMinutes(5) < now,
                "M15" => candle.Time.AddMinutes(15) < now,
                "M30" => candle.Time.AddMinutes(30) < now,
                "H1" => candle.Time.AddHours(1) < now,
                "H4" => candle.Time.AddHours(4) < now,
                "D1" => candle.Time.AddDays(1) < now,
                _ => true
            };
        }
    }
}
