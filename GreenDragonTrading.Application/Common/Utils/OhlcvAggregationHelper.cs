using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Application.Common.Utils
{
    /// <summary>
    /// Helper class để aggregate OHLCV data từ timeframe nhỏ sang lớn
    /// </summary>
    public static class OhlcvAggregationHelper
    {
        /// <summary>
        /// Aggregate từ M1 sang timeframe lớn hơn (M5, M15, M30, H1, H4)
        /// </summary>
        public static List<Ohlcv> AggregateFromM1(List<Ohlcv> m1Data, string targetTimeframe)
        {
            if (!m1Data.Any())
                return new List<Ohlcv>();

            // Validate timeframe
            if (!OhlcvConstants.Timeframes.ComputedFromM1.Contains(targetTimeframe))
                throw new ArgumentException($"Cannot compute {targetTimeframe} from M1 data");

            // Get minutes interval
            var targetMinutes = OhlcvConstants.Timeframes.ToMinutes[targetTimeframe];

            // Group by target timeframe intervals
            var grouped = m1Data
                .OrderBy(o => o.Time)
                .GroupBy(o => RoundDownToTimeframe(o.Time, targetMinutes));

            var result = new List<Ohlcv>();

            foreach (var group in grouped)
            {
                var candles = group.OrderBy(x => x.Time).ToList();

                result.Add(new Ohlcv
                {
                    Time = group.Key,
                    Ticker = candles.First().Ticker,
                    Timeframe = targetTimeframe,
                    Open = candles.First().Open,
                    High = candles.Max(x => x.High),
                    Low = candles.Min(x => x.Low),
                    Close = candles.Last().Close,
                    Volume = candles.Sum(x => x.Volume),
                    Value = candles.Sum(x => x.Value ?? 0),
                    TradesCount = candles.Sum(x => x.TradesCount ?? 0),
                    CreatedAt = DateTime.UtcNow,
                    Source = OhlcvConstants.Sources.Computed
                });
            }

            return result;
        }

        /// <summary>
        /// Aggregate từ D1 sang W1 hoặc MN1
        /// </summary>
        public static List<Ohlcv> AggregateFromD1(List<Ohlcv> d1Data, string targetTimeframe)
        {
            if (!d1Data.Any())
                return new List<Ohlcv>();

            if (!OhlcvConstants.Timeframes.ComputedFromD1.Contains(targetTimeframe))
                throw new ArgumentException($"Cannot compute {targetTimeframe} from D1 data");

            var grouped = targetTimeframe switch
            {
                "W1" => GroupByWeek(d1Data),
                "MN1" => GroupByMonth(d1Data),
                _ => throw new ArgumentException($"Unsupported timeframe: {targetTimeframe}")
            };

            var result = new List<Ohlcv>();

            foreach (var group in grouped)
            {
                var candles = group.Value.OrderBy(x => x.Time).ToList();

                result.Add(new Ohlcv
                {
                    Time = group.Key,
                    Ticker = candles.First().Ticker,
                    Timeframe = targetTimeframe,
                    Open = candles.First().Open,
                    High = candles.Max(x => x.High),
                    Low = candles.Min(x => x.Low),
                    Close = candles.Last().Close,
                    Volume = candles.Sum(x => x.Volume),
                    Value = candles.Sum(x => x.Value ?? 0),
                    TradesCount = candles.Sum(x => x.TradesCount ?? 0),
                    CreatedAt = DateTime.UtcNow,
                    Source = OhlcvConstants.Sources.Computed
                });
            }

            return result;
        }

        private static DateTime RoundDownToTimeframe(DateTime time, int minutes)
        {
            var totalMinutes = time.Hour * 60 + time.Minute;
            var roundedMinutes = (totalMinutes / minutes) * minutes;

            return new DateTime(
                time.Year, time.Month, time.Day,
                roundedMinutes / 60, roundedMinutes % 60, 0,
                time.Kind
            );
        }

        private static Dictionary<DateTime, List<Ohlcv>> GroupByWeek(List<Ohlcv> d1Data)
        {
            return d1Data
                .OrderBy(o => o.Time)
                .GroupBy(o => GetWeekStart(o.Time))
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        private static Dictionary<DateTime, List<Ohlcv>> GroupByMonth(List<Ohlcv> d1Data)
        {
            return d1Data
                .OrderBy(o => o.Time)
                .GroupBy(o => new DateTime(o.Time.Year, o.Time.Month, 1, 0, 0, 0, o.Time.Kind))
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        private static DateTime GetWeekStart(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.Date.AddDays(-diff);
        }
    }
}