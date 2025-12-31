using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Seeding
{
    public static class ModuleLayoutSeeder
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ModuleLayout>().HasData(
                new ModuleLayout
                {
                    Id = 1, 
                    UserId = null, 
                    ModuleType = ModuleType.StockScreener,
                    LayoutName = "Giao diện bộ lọc mặc định",
                    IsSystemDefault = true,
                    ConfigJson = StockScreenerDefaultJson,
                    CreatedAt =  DateTimeOffset.UtcNow,
                    UpdatedAt =  DateTimeOffset.UtcNow
                }
            );
        }

        private const string StockScreenerDefaultJson = @"
        {
          ""state"": {
            ""columns"": {
              ""ticker"": { ""field"": ""ticker"", ""visible"": true, ""width"": 80, ""order"": 0 },
              ""lastPrice"": { ""field"": ""lastPrice"", ""visible"": true, ""width"": 95, ""order"": 10 },
              ""change"": { ""field"": ""change"", ""visible"": true, ""width"": 80, ""order"": 12 },
              ""ratioChange"": { ""field"": ""ratioChange"", ""visible"": true, ""width"": 90, ""order"": 13 },
              ""totalVol"": { ""field"": ""totalVol"", ""visible"": true, ""width"": 120, ""order"": 20 },
              ""PE"": { ""field"": ""PE"", ""visible"": false, ""width"": 80, ""order"": 48 },
              ""ROE"": { ""field"": ""ROE"", ""visible"": false, ""width"": 80, ""order"": 49 }
            }
          }
        }";
    }
}