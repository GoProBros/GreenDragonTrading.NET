using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Seeding
{
    public static class WorkspaceSeeder
    {
        public const int SystemDefaultWebId = 1;
        public const int SystemDefaultMobileId = 2;

        private static readonly DateTimeOffset SeedTime = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        private const string DefaultWebLayoutJson = @"
        {
          ""modules"": [
            {
              ""i"": ""stock-screener-web"",
              ""type"": ""stock-screener"",
              ""title"": ""Bộ lọc cổ phiếu (Web)"",
              ""x"": 0, ""y"": 0, ""w"": 96, ""h"": 20
            }
          ]
        }";

        private const string DefaultMobileLayoutJson = @"
        {
          ""modules"": [
            {
              ""i"": ""stock-list-mobile"",
              ""type"": ""stock-list"",
              ""title"": ""Danh mục theo dõi (Mobile)"",
              ""x"": 0, ""y"": 0, ""w"": 12, ""h"": 10
            }
          ]
        }";

        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Workspace>().HasData(
                new Workspace
                {
                    Id = SystemDefaultWebId,
                    UserId = null,
                    WorkspaceName = "DEFAULT_WEB_LAYOUT",
                    LayoutJson = DefaultWebLayoutJson.Trim(),
                    Type = WorkspaceType.Web, 
                    IsDefault = true,
                    ShareCode = null,
                    CreatedAt = SeedTime,
                    UpdatedAt = SeedTime
                },

                new Workspace
                {
                    Id = SystemDefaultMobileId,
                    UserId = null,
                    WorkspaceName = "DEFAULT_MOBILE_LAYOUT",
                    LayoutJson = DefaultMobileLayoutJson.Trim(),
                    Type = WorkspaceType.Mobile,
                    IsDefault = true,
                    ShareCode = null,
                    CreatedAt = SeedTime,
                    UpdatedAt = SeedTime
                }
            );
        }
    }
}