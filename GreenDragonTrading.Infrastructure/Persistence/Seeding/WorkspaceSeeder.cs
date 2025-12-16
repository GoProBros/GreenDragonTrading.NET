using GreenDragonTrading.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Seeding
{
    public static class WorkspaceSeeder
    {
        // ID cố định cho Layout Master (sử dụng ID số nguyên 1)
        public const int SystemDefaultLayoutId = 1;

        // Dữ liệu JSON mẫu cho Layout mặc định (Thay thế bằng JSON thực tế của bạn)
        private const string DefaultLayoutJson = @"
        {
          ""modules"": [
            { ""id"": ""watchList"", ""position"": { ""x"": 0, ""y"": 0, ""w"": 6, ""h"": 4 } },
            { ""id"": ""chart"", ""position"": { ""x"": 6, ""y"": 0, ""w"": 6, ""h"": 8 } },
            { ""id"": ""orderBook"", ""position"": { ""x"": 0, ""y"": 4, ""w"": 6, ""h"": 4 } }
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
                    UpdateAt = DateTimeOffset.UtcNow
                }
            );
        }
    }
}