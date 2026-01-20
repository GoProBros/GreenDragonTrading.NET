using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace GreenDragonTrading.Infrastructure.Persistence
{
    public class GdtPostgreSqlDbContext(DbContextOptions<GdtPostgreSqlDbContext> options) : DbContext(options)
    {
        public DbSet<Exchange> Exchanges => Set<Exchange>();
        public DbSet<Sector> Sectors => Set<Sector>();
        public DbSet<Symbol> Symbols => Set<Symbol>();
        public DbSet<User> Users => Set<User>();
        public DbSet<FinancialReport> FinancialReports => Set<FinancialReport>();
        public DbSet<Subscription> Subscriptions => Set<Subscription>();
        public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();
        public DbSet<Workspace> Workspaces => Set<Workspace>();
        public DbSet<ModuleLayout> ModuleLayouts => Set<ModuleLayout>();
        public DbSet<WatchList> WatchLists => Set<WatchList>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(builder =>
            {
                builder.ToTable("users");

                builder.Property(u => u.Id)
                        .HasDefaultValueSql("gen_random_uuid()");

                builder.HasIndex(u => u.Email)
                        .IsUnique();

                builder.HasIndex(u => u.Username)
                        .IsUnique();

                builder.HasMany(u => u.UserSubscriptions)
                       .WithOne(us => us.User)
                       .HasForeignKey(us => us.UserId)
                       .IsRequired()
                       .OnDelete(DeleteBehavior.Cascade);

                builder.HasMany(u => u.Workspaces)
                       .WithOne(w => w.User)
                       .HasForeignKey(w => w.UserId)
                       .IsRequired(false)
                       .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Subscription>(builder =>
            {
                builder.ToTable("subscriptions");

                builder.Property(s => s.Price)
                       .HasColumnType("numeric(18, 2)"); 
            });

            modelBuilder.Entity<UserSubscription>(builder =>
            {
                builder.ToTable("user_subscriptions");

                builder.HasOne(us => us.Subscription)
                       .WithMany(s => s.UserSubscriptions)
                       .HasForeignKey(us => us.SubscriptionId)
                       .IsRequired()
                       .OnDelete(DeleteBehavior.Restrict); 
            });

            modelBuilder.Entity<Workspace>(builder =>
            {
                builder.ToTable("workspaces");

                builder.Property(w => w.LayoutJson)
                       .HasColumnType("jsonb");

                builder.HasIndex(w => new { w.UserId, w.WorkspaceName })
                       .IsUnique();

                builder.HasIndex(w => w.ShareCode)
                       .IsUnique()
                       .HasFilter("\"share_code\" IS NOT NULL");

                builder.HasIndex(w => new { w.UserId, w.IsDefault })
           .HasFilter("\"is_default\" = true");
            });

            modelBuilder.Entity<ModuleLayout>(builder =>
            {
                builder.ToTable("module_layouts");

                builder.HasIndex(m => new { m.UserId, m.ModuleType });

                builder.HasIndex(m => m.IsSystemDefault)
                       .HasFilter("\"is_system_default\" = true");

                builder.Property(m => m.ConfigJson)
                       .HasColumnType("jsonb");

                builder.HasOne(m => m.User)
                       .WithMany() 
                       .HasForeignKey(m => m.UserId)
                       .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<WatchList>(builder =>
            {
                builder.ToTable("watch_lists");

                builder.Property(w => w.Tickers)
                       .HasColumnType("jsonb");

                builder.HasIndex(w => w.UserId);

                builder.HasIndex(w => new { w.UserId, w.Name });

                builder.HasOne(w => w.User)
                       .WithMany() 
                       .HasForeignKey(w => w.UserId)
                       .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<FinancialReport>(builder =>
            {
                builder.ToTable("financial_reports");

                builder.Property(f => f.Id)
                       .HasDefaultValueSql("gen_random_uuid()");

                builder.HasOne(f => f.Symbol)
                       .WithMany()
                       .HasForeignKey(f => f.Ticker)
                       .IsRequired()
                       .OnDelete(DeleteBehavior.Restrict);

                builder.HasIndex(f => new { f.Ticker, f.Year, f.Period })
                       .IsUnique();

                builder.HasIndex(f => f.Status);
                
                builder.HasIndex(f => f.FilePath);

                // Configure ReportData as JSONB column with proper serialization
                builder.Property(f => f.ReportData)
                       .HasColumnName("report_data")
                       .HasColumnType("jsonb")
                       .HasConversion(
                           v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                           v => JsonSerializer.Deserialize<FinancialReportData>(v, (JsonSerializerOptions?)null) ?? new FinancialReportData(),
                           new ValueComparer<FinancialReportData>(
                               (c1, c2) => JsonSerializer.Serialize(c1, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(c2, (JsonSerializerOptions?)null),
                               c => JsonSerializer.Serialize(c, (JsonSerializerOptions?)null).GetHashCode(),
                               c => JsonSerializer.Deserialize<FinancialReportData>(JsonSerializer.Serialize(c, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null) ?? new FinancialReportData()
                           )
                       );
            });

            modelBuilder.Entity<Transaction>(builder =>
            {
                builder.ToTable("transactions");

                builder.Property(t => t.Id)
                       .HasDefaultValueSql("gen_random_uuid()");

                builder.Property(t => t.OrderCode)
                       .IsRequired();

                builder.HasIndex(t => t.OrderCode)
                       .IsUnique();

                builder.Property(t => t.Amount)
                       .HasColumnType("numeric(18, 2)");

                builder.Property(t => t.Status)
                       .HasColumnType("smallint")
                       .HasDefaultValue(TransactionStatus.Pending);

                builder.HasOne(t => t.User)
                       .WithMany()
                       .HasForeignKey(t => t.UserId)
                       .OnDelete(DeleteBehavior.Cascade);

                builder.HasOne(t => t.Subscription)
                       .WithMany()
                       .HasForeignKey(t => t.SubscriptionId)
                       .OnDelete(DeleteBehavior.Restrict);
            });

            DatabaseSeeder.SeedAll(modelBuilder);
        }
    }
}
