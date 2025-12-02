using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Seeding
{
    public static class ExchangeSeeder
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Exchange>().HasData(
                new Exchange
                {
                    Code = ExchangeConstant.EXCHANGE_HSX,
                    Name = "Sở Giao dịch Chứng khoán Thành phố Hồ Chí Minh",
                    NormalFluctuationLimit = 0.07m,
                    FirstDayFluctuationLimit = 0.20m,
                    NoRightsFluctuationLimit = 0.20m,
                    Status = CommonStatus.Active
                },
                new Exchange
                {
                    Code = ExchangeConstant.EXCHANGE_HNX,
                    Name = "Sở Giao dịch Chứng khoán Hà Nội",
                    NormalFluctuationLimit = 0.10m,
                    FirstDayFluctuationLimit = 0.30m,
                    NoRightsFluctuationLimit = 0.30m,
                    Status = CommonStatus.Active
                },
                new Exchange
                {
                    Code = ExchangeConstant.EXCHANGE_UPCOM,
                    Name = "Thị trường UPCoM",
                    NormalFluctuationLimit = 0.15m,
                    FirstDayFluctuationLimit = 0.40m,
                    NoRightsFluctuationLimit = 0.40m,
                    Status = CommonStatus.Active
                }
            );
        }
    }
}
