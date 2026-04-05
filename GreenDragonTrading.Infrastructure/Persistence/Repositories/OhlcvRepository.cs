using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace GreenDragonTrading.Infrastructure.Persistence.Repositories
{
    public class OhlcvRepository : IOhlcvRepository
    {
        private readonly OhlcvTimescaleDbContext _context;

        public OhlcvRepository(OhlcvTimescaleDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Ohlcv ohlcv, CancellationToken cancellationToken = default)
        {
            await _context.Ohlcv.AddAsync(ohlcv, cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<Ohlcv> ohlcvList, CancellationToken cancellationToken = default)
        {
            await _context.Ohlcv.AddRangeAsync(ohlcvList, cancellationToken);
        }

        public async Task<int> BulkUpsertAsync(IEnumerable<Ohlcv> ohlcvList, CancellationToken cancellationToken = default)
        {
            var entities = ohlcvList.ToList();
            if (entities.Count == 0) return 0;

            // Guard against duplicate conflict keys in the same SQL statement.
            // PostgreSQL throws 21000 when ON CONFLICT would update the same row twice.
            var deduped = new Dictionary<(DateTime Time, string Ticker, string Timeframe), Ohlcv>();
            foreach (var entity in entities)
            {
                var normalizedTime = entity.Time.Kind == DateTimeKind.Utc
                    ? entity.Time
                    : DateTime.SpecifyKind(entity.Time, DateTimeKind.Utc);
                var normalizedTicker = (entity.Ticker ?? string.Empty).ToUpperInvariant();
                var normalizedTimeframe = (entity.Timeframe ?? string.Empty).ToUpperInvariant();

                entity.Time = normalizedTime;
                entity.Ticker = normalizedTicker;
                entity.Timeframe = normalizedTimeframe;

                deduped[(normalizedTime, normalizedTicker, normalizedTimeframe)] = entity;
            }

            entities = deduped.Values.ToList();

            // Build a single multi-row INSERT … ON CONFLICT DO UPDATE statement per batch.
            // All rows share one round-trip and one DB connection.
            var totalInserted = 0;
            const int batchSize = 200;
            var now = DateTime.UtcNow;

            for (int offset = 0; offset < entities.Count; offset += batchSize)
            {
                var batch = entities.Skip(offset).Take(batchSize).ToList();

                var sqlBuilder = new System.Text.StringBuilder();
                sqlBuilder.Append(
                    "INSERT INTO ohlcv (time, ticker, timeframe, open, high, low, close, volume, value, trades_count, is_preliminary, created_at, source) VALUES ");

                var parameters = new List<NpgsqlParameter>();
                for (int i = 0; i < batch.Count; i++)
                {
                    var e = batch[i];
                    int b = i * 13; // 13 params per row
                    if (i > 0) sqlBuilder.Append(", ");
                    sqlBuilder.Append(
                        $"(@p{b}, @p{b+1}, @p{b+2}, @p{b+3}, @p{b+4}, @p{b+5}, @p{b+6}, @p{b+7}, @p{b+8}, @p{b+9}, @p{b+10}, @p{b+11}, @p{b+12})");

                    var t = e.Time.Kind == DateTimeKind.Utc ? e.Time : DateTime.SpecifyKind(e.Time, DateTimeKind.Utc);
                    parameters.Add(new NpgsqlParameter($"p{b}",   NpgsqlDbType.TimestampTz) { Value = t });
                    parameters.Add(new NpgsqlParameter($"p{b+1}", NpgsqlDbType.Text)        { Value = e.Ticker });
                    parameters.Add(new NpgsqlParameter($"p{b+2}", NpgsqlDbType.Text)        { Value = e.Timeframe });
                    parameters.Add(new NpgsqlParameter($"p{b+3}", NpgsqlDbType.Numeric)     { Value = e.Open });
                    parameters.Add(new NpgsqlParameter($"p{b+4}", NpgsqlDbType.Numeric)     { Value = e.High });
                    parameters.Add(new NpgsqlParameter($"p{b+5}", NpgsqlDbType.Numeric)     { Value = e.Low });
                    parameters.Add(new NpgsqlParameter($"p{b+6}", NpgsqlDbType.Numeric)     { Value = e.Close });
                    parameters.Add(new NpgsqlParameter($"p{b+7}", NpgsqlDbType.Bigint)      { Value = e.Volume });
                    parameters.Add(new NpgsqlParameter($"p{b+8}", NpgsqlDbType.Numeric)     { Value = e.Value.HasValue ? (object)e.Value.Value : DBNull.Value, IsNullable = true });
                    parameters.Add(new NpgsqlParameter($"p{b+9}", NpgsqlDbType.Integer)     { Value = e.TradesCount.HasValue ? (object)e.TradesCount.Value : DBNull.Value, IsNullable = true });
                    parameters.Add(new NpgsqlParameter($"p{b+10}",NpgsqlDbType.Boolean)     { Value = e.IsPreliminary });
                    parameters.Add(new NpgsqlParameter($"p{b+11}",NpgsqlDbType.TimestampTz) { Value = e.CreatedAt == default ? now : e.CreatedAt });
                    parameters.Add(new NpgsqlParameter($"p{b+12}",NpgsqlDbType.Text)        { Value = string.IsNullOrEmpty(e.Source) ? "SSI" : e.Source });
                }

                sqlBuilder.Append(@"
ON CONFLICT (time, ticker, timeframe) DO UPDATE SET
    open         = EXCLUDED.open,
    high         = EXCLUDED.high,
    low          = EXCLUDED.low,
    close        = EXCLUDED.close,
    volume       = EXCLUDED.volume,
    value        = EXCLUDED.value,
    trades_count = EXCLUDED.trades_count,
    source       = EXCLUDED.source");

                totalInserted += await _context.Database.ExecuteSqlRawAsync(
                    sqlBuilder.ToString(), parameters, cancellationToken);
            }

            return totalInserted;
        }

        public async Task UpsertAsync(Ohlcv ohlcv, CancellationToken cancellationToken = default)
        {
            // Use atomic INSERT ... ON CONFLICT DO UPDATE to avoid race conditions
            // when multiple candles for the same (ticker, time, timeframe) arrive concurrently.
            var sql = @"
                INSERT INTO ohlcv (time, ticker, timeframe, open, high, low, close, volume, value, trades_count, is_preliminary, created_at, source)
                VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12)
                ON CONFLICT (time, ticker, timeframe) DO UPDATE SET
                    open         = EXCLUDED.open,
                    high         = EXCLUDED.high,
                    low          = EXCLUDED.low,
                    close        = EXCLUDED.close,
                    volume       = EXCLUDED.volume,
                    value        = EXCLUDED.value,
                    trades_count = EXCLUDED.trades_count,
                    source       = EXCLUDED.source";

            var parameters = new List<NpgsqlParameter>
            {
                new("p0",  NpgsqlDbType.TimestampTz) { Value = ohlcv.Time.Kind == DateTimeKind.Utc ? ohlcv.Time : DateTime.SpecifyKind(ohlcv.Time, DateTimeKind.Utc) },
                new("p1",  NpgsqlDbType.Text)        { Value = ohlcv.Ticker },
                new("p2",  NpgsqlDbType.Text)        { Value = ohlcv.Timeframe },
                new("p3",  NpgsqlDbType.Numeric)     { Value = ohlcv.Open },
                new("p4",  NpgsqlDbType.Numeric)     { Value = ohlcv.High },
                new("p5",  NpgsqlDbType.Numeric)     { Value = ohlcv.Low },
                new("p6",  NpgsqlDbType.Numeric)     { Value = ohlcv.Close },
                new("p7",  NpgsqlDbType.Bigint)      { Value = ohlcv.Volume },
                new("p8",  NpgsqlDbType.Numeric)     { Value = ohlcv.Value.HasValue ? (object)ohlcv.Value.Value : DBNull.Value, IsNullable = true },
                new("p9",  NpgsqlDbType.Integer)     { Value = ohlcv.TradesCount.HasValue ? (object)ohlcv.TradesCount.Value : DBNull.Value, IsNullable = true },
                new("p10", NpgsqlDbType.Boolean)     { Value = ohlcv.IsPreliminary },
                new("p11", NpgsqlDbType.TimestampTz) { Value = ohlcv.CreatedAt == default ? DateTime.UtcNow : ohlcv.CreatedAt },
                new("p12", NpgsqlDbType.Text)        { Value = string.IsNullOrEmpty(ohlcv.Source) ? "SSI_STREAMING" : ohlcv.Source }
            };

            await _context.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
        }

        public async Task<List<Ohlcv>> GetByTickerAndTimeRangeAsync(
            string ticker,
            string timeframe,
            DateTime fromTime,
            DateTime toTime,
            CancellationToken cancellationToken = default)
        {
            var data = await _context.Ohlcv
                .Where(o => o.Ticker == ticker &&
                           o.Timeframe == timeframe &&
                           o.Time >= fromTime &&
                           o.Time <= toTime)
                .OrderBy(o => o.Time)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Ensure all DateTime have Kind=UTC
            foreach (var item in data)
            {
                if (item.Time.Kind != DateTimeKind.Utc)
                {
                    item.Time = DateTime.SpecifyKind(item.Time, DateTimeKind.Utc);
                }
            }

            return data;
        }

        public async Task<Ohlcv?> GetLatestAsync(
            string ticker,
            string timeframe,
            CancellationToken cancellationToken = default)
        {
            var data = await _context.Ohlcv
                .Where(o => o.Ticker == ticker && o.Timeframe == timeframe)
                .OrderByDescending(o => o.Time)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            // Ensure DateTime has Kind=UTC
            if (data != null && data.Time.Kind != DateTimeKind.Utc)
            {
                data.Time = DateTime.SpecifyKind(data.Time, DateTimeKind.Utc);
            }

            return data;
        }

        public async Task<List<Ohlcv>> GetLatestCandlesAsync(
            string ticker,
            string timeframe,
            int limit,
            CancellationToken cancellationToken = default)
        {
            var data = await _context.Ohlcv
                .Where(o => o.Ticker == ticker && o.Timeframe == timeframe)
                .OrderByDescending(o => o.Time)
                .Take(limit)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Ensure all DateTime have Kind=UTC
            foreach (var item in data)
            {
                if (item.Time.Kind != DateTimeKind.Utc)
                {
                    item.Time = DateTime.SpecifyKind(item.Time, DateTimeKind.Utc);
                }
            }

            return data;
        }

        public async Task<bool> ExistsAsync(
            string ticker,
            string timeframe,
            DateTime time,
            CancellationToken cancellationToken = default)
        {
            return await _context.Ohlcv
                .AnyAsync(o =>
                    o.Ticker == ticker &&
                    o.Timeframe == timeframe &&
                    o.Time == time,
                    cancellationToken);
        }

        public async Task<List<string>> GetAvailableTickersAsync(
            string timeframe,
            CancellationToken cancellationToken = default)
        {
            return await _context.Ohlcv
                .Where(o => o.Timeframe == timeframe)
                .Select(o => o.Ticker)
                .Distinct()
                .OrderBy(t => t)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<(DateTime? FirstTime, DateTime? LastTime)> GetTimeRangeAsync(
            string ticker,
            string timeframe,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Ohlcv
                .Where(o => o.Ticker == ticker && o.Timeframe == timeframe)
                .AsNoTracking();

            var firstTime = await query.MinAsync(o => (DateTime?)o.Time, cancellationToken);
            var lastTime = await query.MaxAsync(o => (DateTime?)o.Time, cancellationToken);

            return (firstTime, lastTime);
        }

        public async Task<int> CountAsync(
            string ticker,
            string timeframe,
            DateTime? fromTime = null,
            DateTime? toTime = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Ohlcv
                .Where(o => o.Ticker == ticker && o.Timeframe == timeframe);

            if (fromTime.HasValue)
                query = query.Where(o => o.Time >= fromTime.Value);

            if (toTime.HasValue)
                query = query.Where(o => o.Time <= toTime.Value);

            return await query.CountAsync(cancellationToken);
        }

        public async Task DeleteOlderThanAsync(
            DateTime olderThan,
            CancellationToken cancellationToken = default)
        {
            await _context.Ohlcv
                .Where(o => o.Time < olderThan)
                .ExecuteDeleteAsync(cancellationToken);
        }

        public async Task<DateTime?> GetOldestDateAsync(
            string ticker,
            string timeframe,
            CancellationToken cancellationToken = default)
        {
            return await _context.Ohlcv
                .Where(o => o.Ticker == ticker.ToUpper() && o.Timeframe == timeframe.ToUpper())
                .OrderBy(o => o.Time)
                .Select(o => o.Time)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<DateTime?> GetNewestDateAsync(
            string ticker,
            string timeframe,
            CancellationToken cancellationToken = default)
        {
            return await _context.Ohlcv
                .Where(o => o.Ticker == ticker.ToUpper() && o.Timeframe == timeframe.ToUpper())
                .OrderByDescending(o => o.Time)
                .Select(o => o.Time)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task DeleteByTickerAndTimeframeAsync(
            string ticker,
            string timeframe,
            CancellationToken cancellationToken = default)
        {
            await _context.Ohlcv
                .Where(o => o.Ticker == ticker.ToUpper() && o.Timeframe == timeframe.ToUpper())
                .ExecuteDeleteAsync(cancellationToken);
        }

        public async Task<int> DeleteByTickerTimeframeAndRangeAsync(
            string ticker,
            string timeframe,
            DateTime? fromTime = null,
            DateTime? toTime = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Ohlcv
                .Where(o => o.Ticker == ticker.ToUpper() && o.Timeframe == timeframe.ToUpper());

            if (fromTime.HasValue)
                query = query.Where(o => o.Time >= fromTime.Value);

            if (toTime.HasValue)
                query = query.Where(o => o.Time <= toTime.Value);

            return await query.ExecuteDeleteAsync(cancellationToken);
        }

        public async Task<int> DeleteByTimeframeAndRangeAsync(
            string timeframe,
            DateTime? fromTime = null,
            DateTime? toTime = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Ohlcv
                .Where(o => o.Timeframe == timeframe.ToUpper());

            if (fromTime.HasValue)
                query = query.Where(o => o.Time >= fromTime.Value);

            if (toTime.HasValue)
                query = query.Where(o => o.Time <= toTime.Value);

            return await query.ExecuteDeleteAsync(cancellationToken);
        }
    }
}