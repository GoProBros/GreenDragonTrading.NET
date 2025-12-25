using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence
{
    public class GdtPostgreSqlDbContext(DbContextOptions<GdtPostgreSqlDbContext> options) : DbContext(options)
    {
        public DbSet<Exchange> Exchanges => Set<Exchange>();
        public DbSet<Sector> Sectors => Set<Sector>();
        public DbSet<Symbol> Symbols => Set<Symbol>();

        public DbSet<User> Users => Set<User>();
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
            });

            DatabaseSeeder.SeedAll(modelBuilder);
        }
    }
}
