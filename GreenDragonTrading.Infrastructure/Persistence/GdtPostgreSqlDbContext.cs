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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed data
            DatabaseSeeder.SeedAll(modelBuilder);
        }
    }
}
