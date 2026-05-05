using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenDragonTrading.Infrastructure.Persistence.Seeding
{
    public static class ProactiveAlertLayerBSettingSeeder
    {
        private static readonly DateTimeOffset SeedTime = new(2026, 5, 5, 0, 0, 0, TimeSpan.Zero);

        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProactiveAlertLayerBSetting>().HasData(
                new ProactiveAlertLayerBSetting
                {
                    Id = ProactiveAlertLayerBDefaults.DefaultSettingsId,
                    Timeframe = ProactiveAlertLayerBDefaults.Timeframe,
                    MinAbsoluteMovePercent = ProactiveAlertLayerBDefaults.MinAbsoluteMovePercent,
                    AtrMoveMultiplier = ProactiveAlertLayerBDefaults.AtrMoveMultiplier,
                    MinVolumeRatio = ProactiveAlertLayerBDefaults.MinVolumeRatio,
                    MinAdx = ProactiveAlertLayerBDefaults.MinAdx,
                    MaxIndicatorSnapshotAgeMinutes = ProactiveAlertLayerBDefaults.MaxIndicatorSnapshotAgeMinutes,
                    CreatedAt = SeedTime,
                    UpdatedAt = SeedTime
                });
        }
    }
}
