using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Seeding
{
    /// <summary>
    /// Aggregates all seeders and applies them to the ModelBuilder
    /// </summary>
    public static class DatabaseSeeder
    {
        public static void SeedAll(ModelBuilder modelBuilder)
        {
            ExchangeSeeder.Seed(modelBuilder);
            UserSeeder.Seed(modelBuilder);
            ModuleLayoutSeeder.Seed(modelBuilder);
            WorkspaceSeeder.Seed(modelBuilder);
            AnalysisReportCategorySeeder.Seed(modelBuilder);
            MarketIndexSeeder.Seed(modelBuilder);
        }
    }
}
