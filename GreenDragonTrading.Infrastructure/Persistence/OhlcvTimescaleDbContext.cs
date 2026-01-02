using GreenDragonTrading.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence
{
    public class OhlcvTimescaleDbContext : DbContext
    {
        public OhlcvTimescaleDbContext(DbContextOptions<OhlcvTimescaleDbContext> options)
            : base(options)
        {
        }

        public DbSet<Ohlcv> Ohlcv => Set<Ohlcv>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Ohlcv>(entity =>
            {
                entity.ToTable("ohlcv");

                entity.HasKey(o => new { o.Time, o.Ticker, o.Timeframe });

                entity.HasIndex(o => o.Ticker)
                    .HasDatabaseName("idx_ohlcv_ticker");

                entity.HasIndex(o => new { o.Ticker, o.Timeframe, o.Time })
                    .HasDatabaseName("idx_ohlcv_ticker_tf_time");

                entity.HasIndex(o => o.Time)
                    .HasDatabaseName("idx_ohlcv_time")
                    .IsDescending();

                entity.HasIndex(o => o.CreatedAt)
                    .HasDatabaseName("idx_ohlcv_created_at");
            });
        }
    }
}