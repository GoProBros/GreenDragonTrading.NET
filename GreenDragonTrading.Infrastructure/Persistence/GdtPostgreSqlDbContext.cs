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
                builder.Property(u => u.Id)
                       .HasDefaultValueSql("gen_random_uuid()");

                builder.HasIndex(u => u.Email)
                       .IsUnique();

                builder.HasIndex(u => u.Username)
                       .IsUnique();
            });

            DatabaseSeeder.SeedAll(modelBuilder);
        }
    }
}
