using GreenDragonTrading.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Seeding
{
    public static class WorkspaceSeeder
    {
        public const int SystemDefaultLayoutId = 1;

        private const string DefaultLayoutJson = @"
        {
          ""modules"": [
            {
              ""i"": ""stock-screener-default"",
              ""type"": ""stock-screener"",
              ""title"": ""Bộ lọc cổ phiếu"",
              ""x"": 0, ""y"": 36, ""w"": 96, ""h"": 20,
              ""activeLayoutId"": 1  
            }
          ]
        }";

        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Workspace>().HasData(
                new Workspace
                {
                    Id = SystemDefaultLayoutId,
                    UserId = null, 
                    WorkspaceName = "SYSTEM_DEFAULT_LAYOUT",
                    LayoutJson = DefaultLayoutJson.Trim(),
                    IsDefault = true, 
                    ShareCode = null,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                }
            );
        }
    }
}