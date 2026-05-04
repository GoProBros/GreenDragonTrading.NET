using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Skender.Stock.Indicators;

namespace GreenDragonTrading.Infrastructure.BackgroundWorkers
{
    /// <summary>
    /// Calculates technical indicators on application startup and daily at 15:03 (GMT+7),
    /// then stores latest indicator snapshot in Redis.
    /// </summary>
    public class IndicatorCalculationBackgroundService : BackgroundService
    {
        private const string Timeframe = "D1";
        private const int LookbackCandles = 2000;
        private const int MinQuotesForEma200 = 600;

        private readonly ILogger<IndicatorCalculationBackgroundService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public IndicatorCalculationBackgroundService(
            ILogger<IndicatorCalculationBackgroundService> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Indicator calculation background service started.");

            await RunCalculationCycleAsync("startup", stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var delay = GetDelayUntilNextRun(DateTime.UtcNow);
                    _logger.LogInformation("Next indicator calculation scheduled after {Delay}.", delay);

                    await Task.Delay(delay, stoppingToken);
                    await RunCalculationCycleAsync("daily-15:15", stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in indicator calculation schedule loop.");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            _logger.LogInformation("Indicator calculation background service stopped.");
        }

        private async Task RunCalculationCycleAsync(string trigger, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting indicator calculation cycle. Trigger: {Trigger}", trigger);

            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var ohlcvUow = scope.ServiceProvider.GetRequiredService<IOhlcvUnitOfWork>();
            var redis = scope.ServiceProvider.GetRequiredService<IRedisService>();

            var tickers = await unitOfWork.Symbols.GetAllTickersAsync(cancellationToken);
            var successCount = 0;
            var skipCount = 0;
            var failCount = 0;

            foreach (var ticker in tickers)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    var candles = await ohlcvUow.Ohlcv.GetLatestCandlesAsync(
                        ticker,
                        Timeframe,
                        LookbackCandles,
                        cancellationToken);

                    if (candles.Count < 200)
                    {
                        skipCount++;
                        continue;
                    }

                    var quotes = candles
                        .OrderBy(x => x.Time)
                        .Select(x => new Quote(
                            x.Time,
                            x.Open,
                            x.High,
                            x.Low,
                            x.Close,
                            Convert.ToDecimal(x.Volume)))
                        .ToList();

                    var snapshot = BuildIndicatorSnapshot(ticker.ToUpperInvariant(), Timeframe, quotes);

                    if (snapshot == null)
                    {
                        skipCount++;
                        continue;
                    }

                    var redisKey = RedisConstants.Indicators(snapshot.Ticker, snapshot.Timeframe);
                    await redis.SetHashFieldsAsync(redisKey, ConvertSnapshotToHash(snapshot));

                    var zScoreSnapshot = BuildZScoreSnapshot(snapshot, quotes);
                    if (zScoreSnapshot != null)
                    {
                        var zScoreRedisKey = RedisConstants.IndicatorsZScore(snapshot.Ticker, snapshot.Timeframe);
                        await redis.SetHashFieldsAsync(zScoreRedisKey, ConvertZScoreSnapshotToHash(zScoreSnapshot));
                    }

                    successCount++;
                }
                catch (Exception ex)
                {
                    failCount++;
                    _logger.LogWarning(ex, "Failed calculating indicators for {Ticker}", ticker);
                }
            }

            _logger.LogInformation(
                "Indicator calculation cycle completed. Success={Success}, Skipped={Skipped}, Failed={Failed}",
                successCount,
                skipCount,
                failCount);
        }

        private static IndicatorSnapshotDto? BuildIndicatorSnapshot(string ticker, string timeframe, List<Quote> quotes)
        {
            if (quotes.Count == 0)
            {
                return null;
            }

            var lastQuote = quotes[^1];

            // EMA
            var ema20 = quotes.ToEma(20).LastOrDefault(x => x.Ema.HasValue)?.Ema;
            var ema50 = quotes.ToEma(50).LastOrDefault(x => x.Ema.HasValue)?.Ema;
            var ema200 = quotes.Count >= MinQuotesForEma200
                ? quotes.ToEma(200).LastOrDefault(x => x.Ema.HasValue)?.Ema
                : null;

            // Volume MA20
            var volumeMa20 = quotes.Count >= 20
                ? quotes.TakeLast(20).Average(x => (double)x.Volume)
                : (double?)null;

            // MACD, RSI, BB, ATR, ADX
            var macd = quotes.ToMacd().LastOrDefault(x => x.Macd.HasValue || x.Signal.HasValue);
            var rsi14 = quotes.ToRsi(14).LastOrDefault(x => x.Rsi.HasValue)?.Rsi;
            var bb = quotes.ToBollingerBands(20, 2).LastOrDefault(
                            x => x.Sma.HasValue || x.UpperBand.HasValue || x.LowerBand.HasValue);
            var atr14 = quotes.ToAtr(14).LastOrDefault(x => x.Atr.HasValue)?.Atr;
            var adx14 = quotes.ToAdx(14).LastOrDefault(x => x.Adx.HasValue)?.Adx;

            var close = Convert.ToDecimal(lastQuote.Close);
            var volume = Convert.ToInt64(lastQuote.Volume);
            var bbUpper = ToDecimal(bb?.UpperBand);
            var bbLower = ToDecimal(bb?.LowerBand);
            decimal? bbPercentB = null;
            decimal? volumeToVolumeMa20Ratio = null;
            decimal? volumeMa20Decimal = volumeMa20 is null ? (decimal?)null : Convert.ToDecimal(volumeMa20);

            if (volumeMa20Decimal.HasValue && volumeMa20Decimal.Value > 0)
            {
                volumeToVolumeMa20Ratio = volume / volumeMa20Decimal.Value;
            }

            if (bbUpper.HasValue && bbLower.HasValue && bbUpper.Value > bbLower.Value)
            {
                bbPercentB = (close - bbLower.Value) / (bbUpper.Value - bbLower.Value);
            }

            return new IndicatorSnapshotDto
            {
                Ticker = ticker,
                Timeframe = timeframe,
                CandleTime = lastQuote.Timestamp,
                Close = close,
                Volume = volume,
                Ema20 = ToDecimal(ema20),
                Ema50 = ToDecimal(ema50),
                Ema200 = ToDecimal(ema200),
                VolumeMa20 = volumeMa20Decimal,
                VolumeToVolumeMa20Ratio = volumeToVolumeMa20Ratio,
                Macd = ToDecimal(macd?.Macd),
                MacdSignal = ToDecimal(macd?.Signal),
                MacdHistogram = ToDecimal(macd?.Histogram),
                Rsi14 = ToDecimal(rsi14),
                BollingerMiddle = ToDecimal(bb?.Sma),
                BollingerUpper = bbUpper,
                BollingerLower = bbLower,
                BbPercentB = bbPercentB,
                Atr14 = ToDecimal(atr14),
                Adx14 = ToDecimal(adx14),
                CalculatedAt = DateTime.UtcNow
            };
        }

        private static decimal? ToDecimal(double? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return Convert.ToDecimal(value.Value);
        }

        private static Dictionary<string, object> ConvertSnapshotToHash(IndicatorSnapshotDto snapshot)
        {
            var result = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                [nameof(IndicatorSnapshotDto.Ticker)] = snapshot.Ticker,
                [nameof(IndicatorSnapshotDto.Timeframe)] = snapshot.Timeframe,
                [nameof(IndicatorSnapshotDto.CandleTime)] = snapshot.CandleTime,
                [nameof(IndicatorSnapshotDto.CalculatedAt)] = snapshot.CalculatedAt
            };

            AddIfHasValue(result, nameof(IndicatorSnapshotDto.Close), snapshot.Close);
            if (snapshot.Volume.HasValue)
            {
                result[nameof(IndicatorSnapshotDto.Volume)] = snapshot.Volume.Value;
            }

            AddIfHasValue(result, nameof(IndicatorSnapshotDto.Ema20), snapshot.Ema20);
            AddIfHasValue(result, nameof(IndicatorSnapshotDto.Ema50), snapshot.Ema50);
            AddIfHasValue(result, nameof(IndicatorSnapshotDto.Ema200), snapshot.Ema200);
            AddIfHasValue(result, nameof(IndicatorSnapshotDto.VolumeMa20), snapshot.VolumeMa20);
            AddIfHasValue(result, nameof(IndicatorSnapshotDto.VolumeToVolumeMa20Ratio), snapshot.VolumeToVolumeMa20Ratio);
            AddIfHasValue(result, nameof(IndicatorSnapshotDto.Macd), snapshot.Macd);
            AddIfHasValue(result, nameof(IndicatorSnapshotDto.MacdSignal), snapshot.MacdSignal);
            AddIfHasValue(result, nameof(IndicatorSnapshotDto.MacdHistogram), snapshot.MacdHistogram);
            AddIfHasValue(result, nameof(IndicatorSnapshotDto.Rsi14), snapshot.Rsi14);
            AddIfHasValue(result, nameof(IndicatorSnapshotDto.BollingerMiddle), snapshot.BollingerMiddle);
            AddIfHasValue(result, nameof(IndicatorSnapshotDto.BollingerUpper), snapshot.BollingerUpper);
            AddIfHasValue(result, nameof(IndicatorSnapshotDto.BollingerLower), snapshot.BollingerLower);
            AddIfHasValue(result, nameof(IndicatorSnapshotDto.BbPercentB), snapshot.BbPercentB);
            AddIfHasValue(result, nameof(IndicatorSnapshotDto.Atr14), snapshot.Atr14);
            AddIfHasValue(result, nameof(IndicatorSnapshotDto.Adx14), snapshot.Adx14);

            return result;
        }

        private static IndicatorSnapshotZScoreDto? BuildZScoreSnapshot(IndicatorSnapshotDto snapshot, List<Quote> quotes)
        {
            if (quotes.Count == 0)
            {
                return null;
            }

            var closeSeries = quotes.Select(x => Convert.ToDecimal(x.Close)).ToList();
            var volumeSeries = quotes.Select(x => Convert.ToDecimal(x.Volume)).ToList();

            var ema20Series = quotes
                .ToEma(20)
                .Where(x => x.Ema.HasValue)
                .Select(x => Convert.ToDecimal(x.Ema!.Value))
                .ToList();
            var ema50Series = quotes
                .ToEma(50)
                .Where(x => x.Ema.HasValue)
                .Select(x => Convert.ToDecimal(x.Ema!.Value))
                .ToList();
            var ema200Series = quotes
                .ToEma(200)
                .Where(x => x.Ema.HasValue)
                .Select(x => Convert.ToDecimal(x.Ema!.Value))
                .ToList();

            var volumeMa20Series = BuildVolumeMaSeries(quotes, 20);
            var volumeToVolumeMa20RatioSeries = BuildVolumeToVolumeMaRatioSeries(quotes, 20);

            var macdResults = quotes.ToMacd().ToList();
            var macdSeries = macdResults
                .Where(x => x.Macd.HasValue)
                .Select(x => Convert.ToDecimal(x.Macd!.Value))
                .ToList();
            var macdSignalSeries = macdResults
                .Where(x => x.Signal.HasValue)
                .Select(x => Convert.ToDecimal(x.Signal!.Value))
                .ToList();
            var macdHistogramSeries = macdResults
                .Where(x => x.Histogram.HasValue)
                .Select(x => Convert.ToDecimal(x.Histogram!.Value))
                .ToList();

            var rsi14Series = quotes
                .ToRsi(14)
                .Where(x => x.Rsi.HasValue)
                .Select(x => Convert.ToDecimal(x.Rsi!.Value))
                .ToList();

            var bbResults = quotes.ToBollingerBands(20, 2).ToList();
            var bollingerMiddleSeries = bbResults
                .Where(x => x.Sma.HasValue)
                .Select(x => Convert.ToDecimal(x.Sma!.Value))
                .ToList();
            var bollingerUpperSeries = bbResults
                .Where(x => x.UpperBand.HasValue)
                .Select(x => Convert.ToDecimal(x.UpperBand!.Value))
                .ToList();
            var bollingerLowerSeries = bbResults
                .Where(x => x.LowerBand.HasValue)
                .Select(x => Convert.ToDecimal(x.LowerBand!.Value))
                .ToList();
            var bbPercentBSeries = BuildBbPercentBSeries(quotes, bbResults);

            var atr14Series = quotes
                .ToAtr(14)
                .Where(x => x.Atr.HasValue)
                .Select(x => Convert.ToDecimal(x.Atr!.Value))
                .ToList();
            var adx14Series = quotes
                .ToAdx(14)
                .Where(x => x.Adx.HasValue)
                .Select(x => Convert.ToDecimal(x.Adx!.Value))
                .ToList();

            return new IndicatorSnapshotZScoreDto
            {
                Ticker = snapshot.Ticker,
                Timeframe = snapshot.Timeframe,
                CandleTime = snapshot.CandleTime,
                CalculatedAt = snapshot.CalculatedAt,
                Close = ToZScore(snapshot.Close, closeSeries),
                Volume = snapshot.Volume.HasValue
                    ? ToZScore(Convert.ToDecimal(snapshot.Volume.Value), volumeSeries)
                    : null,
                Ema20 = ToZScore(snapshot.Ema20, ema20Series),
                Ema50 = ToZScore(snapshot.Ema50, ema50Series),
                Ema200 = ToZScore(snapshot.Ema200, ema200Series),
                VolumeMa20 = ToZScore(snapshot.VolumeMa20, volumeMa20Series),
                VolumeToVolumeMa20Ratio = ToZScore(snapshot.VolumeToVolumeMa20Ratio, volumeToVolumeMa20RatioSeries),
                Macd = ToZScore(snapshot.Macd, macdSeries),
                MacdSignal = ToZScore(snapshot.MacdSignal, macdSignalSeries),
                MacdHistogram = ToZScore(snapshot.MacdHistogram, macdHistogramSeries),
                Rsi14 = ToZScore(snapshot.Rsi14, rsi14Series),
                BollingerMiddle = ToZScore(snapshot.BollingerMiddle, bollingerMiddleSeries),
                BollingerUpper = ToZScore(snapshot.BollingerUpper, bollingerUpperSeries),
                BollingerLower = ToZScore(snapshot.BollingerLower, bollingerLowerSeries),
                BbPercentB = ToZScore(snapshot.BbPercentB, bbPercentBSeries),
                Atr14 = ToZScore(snapshot.Atr14, atr14Series),
                Adx14 = ToZScore(snapshot.Adx14, adx14Series)
            };
        }

        private static List<decimal> BuildVolumeMaSeries(List<Quote> quotes, int period)
        {
            var result = new List<decimal>();
            if (quotes.Count < period)
            {
                return result;
            }

            decimal rollingSum = 0;
            var queue = new Queue<decimal>();

            foreach (var quote in quotes)
            {
                var volume = Convert.ToDecimal(quote.Volume);
                queue.Enqueue(volume);
                rollingSum += volume;

                if (queue.Count > period)
                {
                    rollingSum -= queue.Dequeue();
                }

                if (queue.Count == period)
                {
                    result.Add(rollingSum / period);
                }
            }

            return result;
        }

        private static List<decimal> BuildVolumeToVolumeMaRatioSeries(List<Quote> quotes, int period)
        {
            var volumeMaSeries = BuildVolumeMaSeries(quotes, period);
            var result = new List<decimal>();

            if (volumeMaSeries.Count == 0)
            {
                return result;
            }

            for (var i = period - 1; i < quotes.Count; i++)
            {
                var ma = volumeMaSeries[i - (period - 1)];
                if (ma <= 0)
                {
                    continue;
                }

                var volume = Convert.ToDecimal(quotes[i].Volume);
                result.Add(volume / ma);
            }

            return result;
        }

        private static List<decimal> BuildBbPercentBSeries(List<Quote> quotes, List<BollingerBandsResult> bbResults)
        {
            var result = new List<decimal>();

            for (var i = 0; i < quotes.Count && i < bbResults.Count; i++)
            {
                var bb = bbResults[i];
                if (!bb.UpperBand.HasValue || !bb.LowerBand.HasValue)
                {
                    continue;
                }

                var upper = Convert.ToDecimal(bb.UpperBand.Value);
                var lower = Convert.ToDecimal(bb.LowerBand.Value);
                if (upper <= lower)
                {
                    continue;
                }

                var close = Convert.ToDecimal(quotes[i].Close);
                result.Add((close - lower) / (upper - lower));
            }

            return result;
        }

        private static decimal? ToZScore(decimal? value, List<decimal> series)
        {
            if (!value.HasValue || series.Count == 0)
            {
                return null;
            }

            ComputeSeriesStats(series, out var mean, out var stdDev);
            if (stdDev == 0)
            {
                return 0;
            }

            return (value.Value - mean) / stdDev;
        }

        private static void ComputeSeriesStats(List<decimal> series, out decimal mean, out decimal stdDev)
        {
            if (series.Count == 0)
            {
                mean = 0;
                stdDev = 0;
                return;
            }

            var localMean = series.Average();
            var variance = series.Sum(v => (v - localMean) * (v - localMean)) / series.Count;
            mean = localMean;
            stdDev = (decimal)Math.Sqrt((double)variance);
        }

        private static Dictionary<string, object> ConvertZScoreSnapshotToHash(IndicatorSnapshotZScoreDto snapshot)
        {
            var result = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                [nameof(IndicatorSnapshotZScoreDto.Ticker)] = snapshot.Ticker,
                [nameof(IndicatorSnapshotZScoreDto.Timeframe)] = snapshot.Timeframe,
                [nameof(IndicatorSnapshotZScoreDto.CandleTime)] = snapshot.CandleTime,
                [nameof(IndicatorSnapshotZScoreDto.CalculatedAt)] = snapshot.CalculatedAt,
            };

            AddIfHasValue(result, nameof(IndicatorSnapshotZScoreDto.Close), snapshot.Close);
            AddIfHasValue(result, nameof(IndicatorSnapshotZScoreDto.Volume), snapshot.Volume);
            AddIfHasValue(result, nameof(IndicatorSnapshotZScoreDto.Ema20), snapshot.Ema20);
            AddIfHasValue(result, nameof(IndicatorSnapshotZScoreDto.Ema50), snapshot.Ema50);
            AddIfHasValue(result, nameof(IndicatorSnapshotZScoreDto.Ema200), snapshot.Ema200);
            AddIfHasValue(result, nameof(IndicatorSnapshotZScoreDto.VolumeMa20), snapshot.VolumeMa20);
            AddIfHasValue(result, nameof(IndicatorSnapshotZScoreDto.VolumeToVolumeMa20Ratio), snapshot.VolumeToVolumeMa20Ratio);
            AddIfHasValue(result, nameof(IndicatorSnapshotZScoreDto.Macd), snapshot.Macd);
            AddIfHasValue(result, nameof(IndicatorSnapshotZScoreDto.MacdSignal), snapshot.MacdSignal);
            AddIfHasValue(result, nameof(IndicatorSnapshotZScoreDto.MacdHistogram), snapshot.MacdHistogram);
            AddIfHasValue(result, nameof(IndicatorSnapshotZScoreDto.Rsi14), snapshot.Rsi14);
            AddIfHasValue(result, nameof(IndicatorSnapshotZScoreDto.BollingerMiddle), snapshot.BollingerMiddle);
            AddIfHasValue(result, nameof(IndicatorSnapshotZScoreDto.BollingerUpper), snapshot.BollingerUpper);
            AddIfHasValue(result, nameof(IndicatorSnapshotZScoreDto.BollingerLower), snapshot.BollingerLower);
            AddIfHasValue(result, nameof(IndicatorSnapshotZScoreDto.BbPercentB), snapshot.BbPercentB);
            AddIfHasValue(result, nameof(IndicatorSnapshotZScoreDto.Atr14), snapshot.Atr14);
            AddIfHasValue(result, nameof(IndicatorSnapshotZScoreDto.Adx14), snapshot.Adx14);

            return result;
        }

        private static void AddIfHasValue(Dictionary<string, object> map, string key, decimal? value)
        {
            if (value.HasValue)
            {
                map[key] = value.Value;
            }
        }

        private static TimeSpan GetDelayUntilNextRun(DateTime utcNow)
        {
            var vnTz = ResolveVietnamTimeZone();
            var nowVn = TimeZoneInfo.ConvertTimeFromUtc(utcNow, vnTz);

            var nextRunVn = new DateTime(
                nowVn.Year,
                nowVn.Month,
                nowVn.Day,
                15,
                15,
                0,
                DateTimeKind.Unspecified);

            if (nowVn >= nextRunVn)
            {
                nextRunVn = nextRunVn.AddDays(1);
            }

            var nextRunUtc = TimeZoneInfo.ConvertTimeToUtc(nextRunVn, vnTz);
            var delay = nextRunUtc - utcNow;
            return delay > TimeSpan.Zero ? delay : TimeSpan.FromMinutes(1);
        }

        private static TimeZoneInfo ResolveVietnamTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
        }

    }
}