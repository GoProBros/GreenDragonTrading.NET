using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Common.Utils;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Globalization;
using OhlcvEntity = GreenDragonTrading.Domain.Entities.Ohlcv;

namespace GreenDragonTrading.Application.UseCases.Ohlcv.Queries.QueryOhlcvByFields
{
    public class QueryOhlcvByFieldsQueryHandler(
        IOhlcvUnitOfWork ohlcvUow,
        ILogger<QueryOhlcvByFieldsQueryHandler> logger)
        : IRequestHandler<QueryOhlcvByFieldsQuery, ApiResponse<PaginatedResponse<OhlcvFieldQueryItemDto>>>
    {
        private readonly IOhlcvUnitOfWork _ohlcvUow = ohlcvUow;
        private readonly ILogger<QueryOhlcvByFieldsQueryHandler> _logger = logger;

        public async Task<ApiResponse<PaginatedResponse<OhlcvFieldQueryItemDto>>> Handle(
            QueryOhlcvByFieldsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var normalizedTicker = request.Ticker.Trim().ToUpperInvariant();
                var normalizedTimeframe = request.Timeframe.Trim().ToUpperInvariant();
                var fromTime = request.FromTime?.ToUniversalTime() ?? DateTime.UtcNow.AddMonths(-1);
                var toTime = request.ToTime?.ToUniversalTime() ?? DateTime.UtcNow;

                var candles = await QueryCandlesAsync(
                    normalizedTicker,
                    normalizedTimeframe,
                    fromTime,
                    toTime,
                    request.Limit,
                    cancellationToken);

                if (request.Filters.Count > 0)
                {
                    candles = candles
                        .Where(candle => request.Filters.All(filter => MatchFilter(candle, filter)))
                        .ToList();
                }

                var sorted = ApplySorting(candles, request.SortBy, request.SortDirection);

                var totalCount = sorted.Count;

                var paged = sorted
                    .Skip((request.PageIndex - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(MapToDto)
                    .ToList();

                var response = PaginatedResponse<OhlcvFieldQueryItemDto>.Create(
                    paged,
                    totalCount,
                    request.PageIndex,
                    request.PageSize);

                return ApiResponse<PaginatedResponse<OhlcvFieldQueryItemDto>>.Success(
                    response,
                    "Lấy dữ liệu OHLCV theo field thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying OHLCV by fields. Ticker={Ticker}, Timeframe={Timeframe}", request.Ticker, request.Timeframe);
                throw;
            }
        }

        private async Task<List<OhlcvEntity>> QueryCandlesAsync(
            string ticker,
            string timeframe,
            DateTime fromTime,
            DateTime toTime,
            int? limit,
            CancellationToken cancellationToken)
        {
            if (OhlcvConstants.Timeframes.IsStored(timeframe))
            {
                if (limit.HasValue)
                {
                    var latest = await _ohlcvUow.Ohlcv.GetLatestCandlesAsync(
                        ticker,
                        timeframe,
                        limit.Value,
                        cancellationToken);

                    return latest.OrderBy(c => c.Time).ToList();
                }

                return await _ohlcvUow.Ohlcv.GetByTickerAndTimeRangeAsync(
                    ticker,
                    timeframe,
                    fromTime,
                    toTime,
                    cancellationToken);
            }

            var sourceTimeframe = OhlcvConstants.Timeframes.ComputedFromM1.Contains(timeframe)
                ? OhlcvConstants.Timeframes.M1
                : OhlcvConstants.Timeframes.D1;

            var sourceData = await _ohlcvUow.Ohlcv.GetByTickerAndTimeRangeAsync(
                ticker,
                sourceTimeframe,
                fromTime,
                toTime,
                cancellationToken);

            if (sourceData.Count == 0)
            {
                return new List<OhlcvEntity>();
            }

            var aggregated = sourceTimeframe == OhlcvConstants.Timeframes.M1
                ? OhlcvAggregationHelper.AggregateFromM1(sourceData, timeframe)
                : OhlcvAggregationHelper.AggregateFromD1(sourceData, timeframe);

            if (limit.HasValue && aggregated.Count > limit.Value)
            {
                aggregated = aggregated.TakeLast(limit.Value).ToList();
            }

            return aggregated;
        }

        private static List<OhlcvEntity> ApplySorting(
            IEnumerable<OhlcvEntity> candles,
            string sortBy,
            string sortDirection)
        {
            var descending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
            var keySelector = GetFieldSelector(sortBy);

            return descending
                ? candles.OrderByDescending(keySelector).ToList()
                : candles.OrderBy(keySelector).ToList();
        }

        private static Func<OhlcvEntity, object?> GetFieldSelector(string field)
        {
            return NormalizeField(field) switch
            {
                "time" => o => o.Time,
                "ticker" => o => o.Ticker,
                "timeframe" => o => o.Timeframe,
                "open" => o => o.Open,
                "high" => o => o.High,
                "low" => o => o.Low,
                "close" => o => o.Close,
                "volume" => o => o.Volume,
                "value" => o => o.Value,
                "tradescount" => o => o.TradesCount,
                "createdat" => o => o.CreatedAt,
                "source" => o => o.Source,
                "ispreliminary" => o => o.IsPreliminary,
                _ => o => o.Time
            };
        }

        private static bool MatchFilter(OhlcvEntity candle, OhlcvFieldFilterConditionDto filter)
        {
            var value = GetFieldSelector(filter.Field).Invoke(candle);
            var @operator = filter.Operator.Trim().ToLowerInvariant();

            if (@operator == "isnull")
            {
                return value == null;
            }

            if (@operator == "isnotnull")
            {
                return value != null;
            }

            if (value == null || string.IsNullOrWhiteSpace(filter.Value))
            {
                return false;
            }

            return value switch
            {
                decimal decimalValue => CompareDecimal(decimalValue, filter.Value, @operator),
                long longValue => CompareLong(longValue, filter.Value, @operator),
                int intValue => CompareLong(intValue, filter.Value, @operator),
                bool boolValue => CompareBoolean(boolValue, filter.Value, @operator),
                DateTime dateTimeValue => CompareDateTime(dateTimeValue, filter.Value, @operator),
                string stringValue => CompareString(stringValue, filter.Value, @operator),
                _ => CompareString(value.ToString() ?? string.Empty, filter.Value, @operator)
            };
        }

        private static bool CompareDecimal(decimal left, string rightRaw, string @operator)
        {
            if (!decimal.TryParse(rightRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var right))
            {
                return false;
            }

            return @operator switch
            {
                "eq" => left == right,
                "neq" => left != right,
                "gt" => left > right,
                "gte" => left >= right,
                "lt" => left < right,
                "lte" => left <= right,
                _ => false
            };
        }

        private static bool CompareLong(long left, string rightRaw, string @operator)
        {
            if (!long.TryParse(rightRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var right))
            {
                return false;
            }

            return @operator switch
            {
                "eq" => left == right,
                "neq" => left != right,
                "gt" => left > right,
                "gte" => left >= right,
                "lt" => left < right,
                "lte" => left <= right,
                _ => false
            };
        }

        private static bool CompareBoolean(bool left, string rightRaw, string @operator)
        {
            if (!bool.TryParse(rightRaw, out var right))
            {
                return false;
            }

            return @operator switch
            {
                "eq" => left == right,
                "neq" => left != right,
                _ => false
            };
        }

        private static bool CompareDateTime(DateTime left, string rightRaw, string @operator)
        {
            if (!DateTime.TryParse(
                rightRaw,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var right))
            {
                return false;
            }

            return @operator switch
            {
                "eq" => left == right,
                "neq" => left != right,
                "gt" => left > right,
                "gte" => left >= right,
                "lt" => left < right,
                "lte" => left <= right,
                _ => false
            };
        }

        private static bool CompareString(string left, string right, string @operator)
        {
            return @operator switch
            {
                "eq" => string.Equals(left, right, StringComparison.OrdinalIgnoreCase),
                "neq" => !string.Equals(left, right, StringComparison.OrdinalIgnoreCase),
                "contains" => left.Contains(right, StringComparison.OrdinalIgnoreCase),
                _ => false
            };
        }

        private static string NormalizeField(string field)
        {
            return field.Replace("_", string.Empty, StringComparison.Ordinal)
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .Trim()
                .ToLowerInvariant();
        }

        private static OhlcvFieldQueryItemDto MapToDto(OhlcvEntity candle)
        {
            return new OhlcvFieldQueryItemDto
            {
                Time = candle.Time,
                Ticker = candle.Ticker,
                Timeframe = candle.Timeframe,
                Open = candle.Open,
                High = candle.High,
                Low = candle.Low,
                Close = candle.Close,
                Volume = candle.Volume,
                Value = candle.Value,
                TradesCount = candle.TradesCount,
                CreatedAt = candle.CreatedAt,
                Source = candle.Source,
                IsPreliminary = candle.IsPreliminary
            };
        }
    }
}
