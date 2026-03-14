using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Infrastructure.BackgroundWorkers
{
    public partial class SsiStreamingBackgroundService
    {
        /// <summary>
        /// Handles Channel B (OHLCV) updates and fans out to all supported timeframes.
        /// </summary>
        /// <param name="redis">Redis service abstraction.</param>
        /// <param name="response">Deserialized OHLCV payload from SSI.</param>
        private async Task HandleOhlcvData(IRedisService redis, OhlcvDataResponse? response)
        {
            try
            {
                if (response == null || string.IsNullOrWhiteSpace(response.Symbol))
                {
                    _logger.LogWarning("Received null or invalid OHLCV data");
                    return;
                }

                // TradingTime from SSI can be absent; use current server UTC for bucketing.
                var now = DateTime.UtcNow;
                var ticker = response.Symbol.ToUpper();

                var ohlcvUpdate = new OhlcvUpdateData(
                    Open: (decimal)(response.Open ?? 0),
                    High: (decimal)(response.High ?? 0),
                    Low: (decimal)(response.Low ?? 0),
                    Close: (decimal)(response.Close ?? 0),
                    Volume: (long)(response.Volume ?? 0),
                    TotalValue: (decimal)(response.Value ?? 0));

                // Update all configured timeframes directly from SSI stream.
                var timeframes = new[] { "M1", "M5", "M15", "M30", "H1", "H4", "D1", "W1", "MN1" };

                foreach (var timeframe in timeframes)
                {
                    await UpdateTimeframeCandle(redis, ticker, timeframe, ohlcvUpdate, now);
                }

                _logger.LogDebug(
                    "SSI Update: {Ticker} | O:{Open} H:{High} L:{Low} C:{Close} V:{Volume}",
                    ticker, ohlcvUpdate.Open, ohlcvUpdate.High, ohlcvUpdate.Low, ohlcvUpdate.Close, ohlcvUpdate.Volume);
            }
            catch (TaskCanceledException)
            {
                return;
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling OHLCV data for symbol: {Symbol}", response?.Symbol);
            }
        }

        /// <summary>
        /// Updates one timeframe candle state using incremental OHLCV stream data.
        /// </summary>
        /// <param name="redis">Redis service abstraction.</param>
        /// <param name="ticker">Symbol ticker.</param>
        /// <param name="timeframe">Target timeframe (M1..MN1).</param>
        /// <param name="ohlcvData">Incoming OHLCV values.</param>
        /// <param name="now">Current UTC timestamp used for period boundaries.</param>
        private async Task UpdateTimeframeCandle(
            IRedisService redis,
            string ticker,
            string timeframe,
            OhlcvUpdateData ohlcvData,
            DateTime now)
        {
            try
            {
                string redisKey = $"OHLCV:{ticker}:{timeframe}";

                var semaphore = _redisLocks.GetOrAdd(redisKey, _ => new SemaphoreSlim(1, 1));

                if (!await semaphore.WaitAsync(TimeSpan.FromSeconds(5)))
                {
                    // Skip when lock is contended to avoid piling up delayed updates.
                    return;
                }

                try
                {
                    var existingCandle = await redis.GetAsync<CurrentCandleDto>(redisKey);
                    var periodStart = GetPeriodStartTime(now, timeframe);

                    CurrentCandleDto candle;

                    if (existingCandle == null)
                    {
                        // Avoid creating ghost candles from replay payloads outside trading hours.
                        if (!IsWithinTradingHours(now))
                        {
                            return;
                        }

                        candle = new CurrentCandleDto
                        {
                            Ticker = ticker,
                            Timeframe = timeframe,
                            StartTime = periodStart,
                            Open = ohlcvData.Close,
                            High = ohlcvData.Close,
                            Low = ohlcvData.Close,
                            Close = ohlcvData.Close,
                            Volume = ohlcvData.Volume,
                            TotalValue = ohlcvData.TotalValue,
                            LastUpdateTime = now,
                            IsComplete = false,
                        };
                    }
                    else if (existingCandle.StartTime < periodStart)
                    {
                        // Roll to next period only during active trading hours.
                        if (!IsWithinTradingHours(now))
                        {
                            return;
                        }

                        if (ShouldSaveCompletedCandle(timeframe, existingCandle, now))
                        {
                            _saveCandleChannel.Writer.TryWrite(existingCandle);
                        }

                        candle = new CurrentCandleDto
                        {
                            Ticker = ticker,
                            Timeframe = timeframe,
                            StartTime = periodStart,
                            Open = ohlcvData.Close,
                            High = ohlcvData.Close,
                            Low = ohlcvData.Close,
                            Close = ohlcvData.Close,
                            Volume = ohlcvData.Volume,
                            TotalValue = ohlcvData.TotalValue,
                            LastUpdateTime = now,
                            IsComplete = false,
                        };
                    }
                    else
                    {
                        candle = existingCandle;
                        candle.High = Math.Max(candle.High, ohlcvData.Close);
                        candle.Low = candle.Low == 0 ? ohlcvData.Close : Math.Min(candle.Low, ohlcvData.Close);
                        candle.Close = ohlcvData.Close;
                        candle.Volume += ohlcvData.Volume;
                        candle.TotalValue += ohlcvData.TotalValue;
                        candle.LastUpdateTime = now;
                    }

                    var ttl = GetRedisTTL(timeframe);
                    QueueRedisStringWrite(redisKey, candle, ttl);
                    QueueOhlcvBroadcast(candle);
                }
                finally
                {
                    semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update {Timeframe} for {Ticker}", timeframe, ticker);
            }
        }

        /// <summary>
        /// Calculates period start timestamp for a given timeframe.
        /// </summary>
        /// <param name="now">Current UTC timestamp.</param>
        /// <param name="timeframe">Target timeframe (M1..MN1).</param>
        /// <returns>UTC start time of current period.</returns>
        private static DateTime GetPeriodStartTime(DateTime now, string timeframe)
        {
            return timeframe switch
            {
                "M1" => new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, DateTimeKind.Utc),
                "M5" => RoundDownToMinutes(now, 5),
                "M15" => RoundDownToMinutes(now, 15),
                "M30" => RoundDownToMinutes(now, 30),
                "H1" => new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc),
                "H4" => RoundDownToHours(now, 4),
                "D1" => new DateTime(now.AddHours(7).Year, now.AddHours(7).Month, now.AddHours(7).Day, 8, 0, 0, DateTimeKind.Utc),
                "W1" => GetWeekStart(now),
                "MN1" => new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                _ => now,
            };
        }

        /// <summary>
        /// Rounds time down to nearest N-minute bucket.
        /// </summary>
        private static DateTime RoundDownToMinutes(DateTime time, int intervalMinutes)
        {
            var totalMinutes = time.Hour * 60 + time.Minute;
            var roundedMinutes = (totalMinutes / intervalMinutes) * intervalMinutes;
            var hour = roundedMinutes / 60;
            var minute = roundedMinutes % 60;
            return new DateTime(time.Year, time.Month, time.Day, hour, minute, 0, DateTimeKind.Utc);
        }

        /// <summary>
        /// Rounds time down to nearest N-hour bucket.
        /// </summary>
        private static DateTime RoundDownToHours(DateTime time, int intervalHours)
        {
            var roundedHour = (time.Hour / intervalHours) * intervalHours;
            return new DateTime(time.Year, time.Month, time.Day, roundedHour, 0, 0, DateTimeKind.Utc);
        }

        /// <summary>
        /// Checks whether current UTC time falls in VN trading sessions (Mon-Fri, UTC+7).
        /// </summary>
        private static bool IsWithinTradingHours(DateTime utcNow)
        {
            var vn = utcNow.AddHours(7);
            if (vn.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                return false;
            }

            var t = vn.TimeOfDay;
            return (t >= new TimeSpan(9, 0, 0) && t <= new TimeSpan(11, 30, 0))
                || (t >= new TimeSpan(13, 0, 0) && t <= new TimeSpan(15, 15, 0));
        }

        /// <summary>
        /// Returns Monday 00:00 of the week containing provided timestamp.
        /// </summary>
        private static DateTime GetWeekStart(DateTime time)
        {
            var daysSinceMonday = ((int)time.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            return time.Date.AddDays(-daysSinceMonday);
        }

        /// <summary>
        /// Determines whether a completed candle should be persisted to database.
        /// </summary>
        /// <param name="timeframe">Candle timeframe.</param>
        /// <param name="completedCandle">Completed candle snapshot.</param>
        /// <param name="now">Current UTC timestamp.</param>
        /// <returns>True if candle should be saved.</returns>
        private static bool ShouldSaveCompletedCandle(string timeframe, CurrentCandleDto completedCandle, DateTime now)
        {
            if (timeframe == "M1")
            {
                return true;
            }

            if (timeframe == "D1")
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns Redis TTL by timeframe.
        /// </summary>
        /// <param name="timeframe">Candle timeframe.</param>
        /// <returns>Recommended cache lifetime for this timeframe.</returns>
        private static TimeSpan GetRedisTTL(string timeframe)
        {
            return timeframe switch
            {
            // M1 TTL must survive session breaks and overnight gap so first tick next session
            // can still compare against prior state and flush correctly.
                "M1" => TimeSpan.FromHours(24),
                "M5" => TimeSpan.FromMinutes(30),
                "M15" => TimeSpan.FromHours(1),
                "M30" => TimeSpan.FromHours(2),
                "H1" => TimeSpan.FromHours(4),
                "H4" => TimeSpan.FromHours(8),
                "D1" => TimeSpan.FromHours(24),
                "W1" => TimeSpan.FromDays(7),
                "MN1" => TimeSpan.FromDays(30),
                _ => TimeSpan.FromHours(1),
            };
        }
    }
}
