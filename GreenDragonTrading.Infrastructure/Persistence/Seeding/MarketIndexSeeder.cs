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

                // ── HOSE broad market indices ─────────────────────────────────────
                new MarketIndex
                {
                    Code = MarketIndexConstant.VNINDEX,
                    Name = "Chỉ số VN-Index",
                    ExchangeCode = ExchangeConstant.EXCHANGE_HSX,
                    Description = "Chỉ số tổng hợp của toàn bộ cổ phiếu niêm yết trên Sở Giao dịch Chứng khoán TP.HCM (HOSE).",
                    IsBenchmark = true,
                    Status = CommonStatus.Active,
                    CreatedAt = SeedDate
                },
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
                    Code = MarketIndexConstant.VNDIAMOND,
                    Name = "Chỉ số VN Diamond",
                    ExchangeCode = ExchangeConstant.EXCHANGE_HSX,
                    Description = "Rổ cổ phiếu có vốn hóa lớn và tỷ lệ sở hữu nước ngoài cao trên HOSE.",
                    IsBenchmark = false,
                    Status = CommonStatus.Active,
                    CreatedAt = SeedDate
                },

                // ── HOSE sector indices ──────────────────────────────────────────
                new MarketIndex
                {
                    Code = MarketIndexConstant.VNCOND,
                    Name = "Chỉ số VN Consumer Discretionary",
                    ExchangeCode = ExchangeConstant.EXCHANGE_HSX,
                    Description = "Chỉ số ngành Hàng tiêu dùng không thiết yếu trên HOSE.",
                    IsBenchmark = false,
                    Status = CommonStatus.Active,
                    CreatedAt = SeedDate
                },
                new MarketIndex
                {
                    Code = MarketIndexConstant.VNCONS,
                    Name = "Chỉ số VN Consumer Staples",
                    ExchangeCode = ExchangeConstant.EXCHANGE_HSX,
                    Description = "Chỉ số ngành Hàng tiêu dùng thiết yếu trên HOSE.",
                    IsBenchmark = false,
                    Status = CommonStatus.Active,
                    CreatedAt = SeedDate
                },
                new MarketIndex
                {
                    Code = MarketIndexConstant.VNENE,
                    Name = "Chỉ số VN Energy",
                    ExchangeCode = ExchangeConstant.EXCHANGE_HSX,
                    Description = "Chỉ số ngành Năng lượng trên HOSE.",
                    IsBenchmark = false,
                    Status = CommonStatus.Active,
                    CreatedAt = SeedDate
                },
                new MarketIndex
                {
                    Code = MarketIndexConstant.VNFIN,
                    Name = "Chỉ số VN Financials",
                    ExchangeCode = ExchangeConstant.EXCHANGE_HSX,
                    Description = "Chỉ số ngành Tài chính trên HOSE.",
                    IsBenchmark = false,
                    Status = CommonStatus.Active,
                    CreatedAt = SeedDate
                },
                new MarketIndex
                {
                    Code = MarketIndexConstant.VNFINLEAD,
                    Name = "Chỉ số VN Financial Leaders",
                    ExchangeCode = ExchangeConstant.EXCHANGE_HSX,
                    Description = "Rổ cổ phiếu dẫn đầu ngành Tài chính trên HOSE.",
                    IsBenchmark = false,
                    Status = CommonStatus.Active,
                    CreatedAt = SeedDate
                },
                new MarketIndex
                {
                    Code = MarketIndexConstant.VNFINSELECT,
                    Name = "Chỉ số VN Financial Select",
                    ExchangeCode = ExchangeConstant.EXCHANGE_HSX,
                    Description = "Rổ cổ phiếu tuyển chọn ngành Tài chính trên HOSE.",
                    IsBenchmark = false,
                    Status = CommonStatus.Active,
                    CreatedAt = SeedDate
                },
                new MarketIndex
                {
                    Code = MarketIndexConstant.VNHEAL,
                    Name = "Chỉ số VN Healthcare",
                    ExchangeCode = ExchangeConstant.EXCHANGE_HSX,
                    Description = "Chỉ số ngành Y tế và Dược phẩm trên HOSE.",
                    IsBenchmark = false,
                    Status = CommonStatus.Active,
                    CreatedAt = SeedDate
                },
                new MarketIndex
                {
                    Code = MarketIndexConstant.VNIND,
                    Name = "Chỉ số VN Industrials",
                    ExchangeCode = ExchangeConstant.EXCHANGE_HSX,
                    Description = "Chỉ số ngành Công nghiệp trên HOSE.",
                    IsBenchmark = false,
                    Status = CommonStatus.Active,
                    CreatedAt = SeedDate
                },
                new MarketIndex
                {
                    Code = MarketIndexConstant.VNIT,
                    Name = "Chỉ số VN Information Technology",
                    ExchangeCode = ExchangeConstant.EXCHANGE_HSX,
                    Description = "Chỉ số ngành Công nghệ thông tin trên HOSE.",
                    IsBenchmark = false,
                    Status = CommonStatus.Active,
                    CreatedAt = SeedDate
                },
                new MarketIndex
                {
                    Code = MarketIndexConstant.VNMAT,
                    Name = "Chỉ số VN Materials",
                    ExchangeCode = ExchangeConstant.EXCHANGE_HSX,
                    Description = "Chỉ số ngành Nguyên vật liệu trên HOSE.",
                    IsBenchmark = false,
                    Status = CommonStatus.Active,
                    CreatedAt = SeedDate
                },
                new MarketIndex
                {
                    Code = MarketIndexConstant.VNREAL,
                    Name = "Chỉ số VN Real Estate",
                    ExchangeCode = ExchangeConstant.EXCHANGE_HSX,
                    Description = "Chỉ số ngành Bất động sản trên HOSE.",
                    IsBenchmark = false,
                    Status = CommonStatus.Active,
                    CreatedAt = SeedDate
                },
                new MarketIndex
                {
                    Code = MarketIndexConstant.VNUTI,
                    Name = "Chỉ số VN Utilities",
                    ExchangeCode = ExchangeConstant.EXCHANGE_HSX,
                    Description = "Chỉ số ngành Tiện ích công cộng trên HOSE.",
                    IsBenchmark = false,
                    Status = CommonStatus.Active,
                    CreatedAt = SeedDate
                },

                // ── HOSE cross-market indices ────────────────────────────────────
                new MarketIndex
                {
                    Code = MarketIndexConstant.VNX50,
                    Name = "Chỉ số VNX50",
                    ExchangeCode = ExchangeConstant.EXCHANGE_HSX,
                    Description = "Rổ 50 cổ phiếu hàng đầu trên cả HOSE và HNX.",
                    IsBenchmark = false,
                    Status = CommonStatus.Active,
                    CreatedAt = SeedDate
                },
                new MarketIndex
                {
                    Code = MarketIndexConstant.VNXALL,
                    Name = "Chỉ số VNX All-Share",
                    ExchangeCode = ExchangeConstant.EXCHANGE_HSX,
                    Description = "Chỉ số bao gồm toàn bộ cổ phiếu niêm yết trên cả HOSE và HNX.",
                    IsBenchmark = false,
                    Status = CommonStatus.Active,
                    CreatedAt = SeedDate
                },

                // ── HNX indices ──────────────────────────────────────────────────
                new MarketIndex
                {
                    Code = MarketIndexConstant.HNXINDEX,
                    Name = "Chỉ số HNX-Index",
                    ExchangeCode = ExchangeConstant.EXCHANGE_HNX,
                    Description = "Chỉ số tổng hợp của toàn bộ cổ phiếu niêm yết trên Sở Giao dịch Chứng khoán Hà Nội (HNX).",
                    IsBenchmark = true,
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
                },

                // ── UPCOM indices ────────────────────────────────────────────────
                new MarketIndex
                {
                    Code = MarketIndexConstant.HNXUPCOMINDEX,
                    Name = "Chỉ số HNX UPCOM-Index",
                    ExchangeCode = ExchangeConstant.EXCHANGE_UPCOM,
                    Description = "Chỉ số tổng hợp của toàn bộ cổ phiếu đăng ký giao dịch trên thị trường UPCOM.",
                    IsBenchmark = true,
                    Status = CommonStatus.Active,
                    CreatedAt = SeedDate
                }
            );
        }
    }
}
