using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

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

            // Use raw SQL with INSERT ... ON CONFLICT DO NOTHING for proper UPSERT
            // This efficiently handles duplicates without exceptions
            var insertedCount = 0;
            
            // Process in smaller batches of 500 to avoid memory issues
            var batchSize = 500;
            var now = DateTime.UtcNow; // Single timestamp for entire batch
            
            for (int i = 0; i < entities.Count; i += batchSize)
            {
                var batch = entities.Skip(i).Take(batchSize).ToList();
                
                // Build the SQL command for batch insert - include created_at and source (required NOT NULL columns)
                var sql = @"INSERT INTO ohlcv (time, ticker, timeframe, open, high, low, close, volume, is_preliminary, created_at, source) 
                           VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10) 
                           ON CONFLICT (time, ticker, timeframe) DO NOTHING";

                // Execute each entity in the batch
                foreach (var entity in batch)
                {
                    var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                        sql,
                        new object[] 
                        {
                            entity.Time,
                            entity.Ticker,
                            entity.Timeframe,
                            entity.Open,
                            entity.High,
                            entity.Low,
                            entity.Close,
                            entity.Volume,
                            entity.IsPreliminary,
                            entity.CreatedAt == default ? now : entity.CreatedAt, // Use entity's CreatedAt or fallback to now
                            string.IsNullOrEmpty(entity.Source) ? "SSI" : entity.Source // Default to "SSI" if not set
                        },
                        cancellationToken);
                    
                    insertedCount += rowsAffected;
                }
            }

            return insertedCount;
        }

        public async Task UpsertAsync(Ohlcv ohlcv, CancellationToken cancellationToken = default)
        {
            // Check if exists
            var existing = await _context.Ohlcv
                .FirstOrDefaultAsync(o =>
                    o.Time == ohlcv.Time &&
                    o.Ticker == ohlcv.Ticker &&
                    o.Timeframe == ohlcv.Timeframe,
                    cancellationToken);

            if (existing != null)
            {
                // Update existing
                existing.Open = ohlcv.Open;
                existing.High = ohlcv.High;
                existing.Low = ohlcv.Low;
                existing.Close = ohlcv.Close;
                existing.Volume = ohlcv.Volume;
                existing.Value = ohlcv.Value;
                existing.TradesCount = ohlcv.TradesCount;
                existing.Source = ohlcv.Source;
                // Don't update CreatedAt
            }
            else
            {
                // Insert new
                await _context.Ohlcv.AddAsync(ohlcv, cancellationToken);
            }
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
    }
}