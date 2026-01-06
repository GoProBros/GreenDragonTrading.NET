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
            return await _context.Ohlcv
                .Where(o => o.Ticker == ticker &&
                           o.Timeframe == timeframe &&
                           o.Time >= fromTime &&
                           o.Time <= toTime)
                .OrderBy(o => o.Time)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Ohlcv?> GetLatestAsync(
            string ticker,
            string timeframe,
            CancellationToken cancellationToken = default)
        {
            return await _context.Ohlcv
                .Where(o => o.Ticker == ticker && o.Timeframe == timeframe)
                .OrderByDescending(o => o.Time)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<Ohlcv>> GetLatestCandlesAsync(
            string ticker,
            string timeframe,
            int limit,
            CancellationToken cancellationToken = default)
        {
            return await _context.Ohlcv
                .Where(o => o.Ticker == ticker && o.Timeframe == timeframe)
                .OrderByDescending(o => o.Time)
                .Take(limit)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
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