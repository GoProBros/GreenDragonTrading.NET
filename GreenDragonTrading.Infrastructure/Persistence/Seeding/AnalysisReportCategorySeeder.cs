using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeds initial data for AnalysisReportCategory entity
/// </summary>
public static class AnalysisReportCategorySeeder
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        var categories = new List<AnalysisReportCategory>
        {
            new()
            {
                Code = "1000",
                Name = "Báo cáo Vĩ mô",
                Description = "Phân tích kinh tế vĩ mô, chính sách, triển vọng thị trường",
                Level = 1,
                ParentId = null,
                Status = CommonStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Code = "2000",
                Name = "Báo cáo Ngành",
                Description = "Phân tích các ngành công nghiệp, xu hướng và triển vọng",
                Level = 1,
                ParentId = null,
                Status = CommonStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Code = "3000",
                Name = "Báo cáo Doanh nghiệp",
                Description = "Phân tích chuyên sâu về doanh nghiệp niêm yết",
                Level = 1,
                ParentId = null,
                Status = CommonStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Code = "4000",
                Name = "Chiến lược Đầu tư",
                Description = "Chiến lược và khuyến nghị đầu tư",
                Level = 1,
                ParentId = null,
                Status = CommonStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Code = "5000",
                Name = "Báo cáo Thị trường",
                Description = "Nhận định và phân tích thị trường chứng khoán",
                Level = 1,
                ParentId = null,
                Status = CommonStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        modelBuilder.Entity<AnalysisReportCategory>().HasData(categories);
    }
}
