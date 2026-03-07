using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Seeding
{
    public static class MarketIndexSeeder
    {
        private static readonly DateTime SeedDate = new(2026, 3, 7, 0, 0, 0, DateTimeKind.Utc);

        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MarketIndex>().HasData(
                new MarketIndex
                {
                    Code = MarketIndexConstant.VN30,
                    Name = "Chỉ số VN30",
                    ExchangeCode = ExchangeConstant.EXCHANGE_HSX,
                    Description = "Rổ 30 cổ phiếu có vốn hóa lớn nhất và thanh khoản cao nhất trên HOSE.",
                    IsBenchmark = true,
                    Status = CommonStatus.Active,
                    CreatedAt = SeedDate
                },
                new MarketIndex
                {
                    Code = MarketIndexConstant.VN100,
                    Name = "Chỉ số VN100",
                    ExchangeCode = ExchangeConstant.EXCHANGE_HSX,
                    Description = "Rổ 100 cổ phiếu có vốn hóa lớn nhất và thanh khoản cao nhất trên HOSE.",
                    IsBenchmark = false,
                    Status = CommonStatus.Active,
                    CreatedAt = SeedDate
                },
                new MarketIndex
                {
                    Code = MarketIndexConstant.VNMID,
                    Name = "Chỉ số VN Mid-Cap",
                    ExchangeCode = ExchangeConstant.EXCHANGE_HSX,
                    Description = "Chỉ số vốn hóa vừa, bao gồm các cổ phiếu từ hạng 31 đến 100 trên HOSE.",
                    IsBenchmark = false,
                    Status = CommonStatus.Active,
                    CreatedAt = SeedDate
                },
                new MarketIndex
                {
                    Code = MarketIndexConstant.VNSML,
                    Name = "Chỉ số VN Small-Cap",
                    ExchangeCode = ExchangeConstant.EXCHANGE_HSX,
                    Description = "Chỉ số vốn hóa nhỏ, bao gồm các cổ phiếu từ hạng 101 trở xuống trên HOSE.",
                    IsBenchmark = false,
                    Status = CommonStatus.Active,
                    CreatedAt = SeedDate
                },
                new MarketIndex
                {
                    Code = MarketIndexConstant.VNALL,
                    Name = "Chỉ số VN All-Share",
                    ExchangeCode = ExchangeConstant.EXCHANGE_HSX,
                    Description = "Chỉ số bao gồm toàn bộ cổ phiếu niêm yết trên HOSE.",
                    IsBenchmark = false,
                    Status = CommonStatus.Active,
                    CreatedAt = SeedDate
                },
                new MarketIndex
                {
                    Code = MarketIndexConstant.VNSI,
                    Name = "Chỉ số VN Sustainability",
                    ExchangeCode = ExchangeConstant.EXCHANGE_HSX,
                    Description = "Chỉ số phát triển bền vững, bao gồm các doanh nghiệp đạt tiêu chí ESG trên HOSE.",
                    IsBenchmark = false,
                    Status = CommonStatus.Active,
                    CreatedAt = SeedDate
                },
                new MarketIndex
                {
                    Code = MarketIndexConstant.HNX30,
                    Name = "Chỉ số HNX30",
                    ExchangeCode = ExchangeConstant.EXCHANGE_HNX,
                    Description = "Rổ 30 cổ phiếu có vốn hóa lớn nhất và thanh khoản cao nhất trên HNX.",
                    IsBenchmark = true,
                    Status = CommonStatus.Active,
                    CreatedAt = SeedDate
                }
            );
        }
    }
}
