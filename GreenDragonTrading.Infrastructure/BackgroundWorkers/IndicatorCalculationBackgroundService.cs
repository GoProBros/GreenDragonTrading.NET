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