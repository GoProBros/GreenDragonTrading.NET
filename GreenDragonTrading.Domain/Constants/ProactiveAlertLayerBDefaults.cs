namespace GreenDragonTrading.Domain.Constants
{
    public static class ProactiveAlertLayerBDefaults
    {
        public const int DefaultSettingsId = 1;
        public const string Timeframe = "D1";
        public const decimal MinAbsoluteMovePercent = 1.0m;
        public const decimal AtrMoveMultiplier = 0.7m;
        public const decimal MinVolumeRatio = 1.8m;
        public const decimal MinAdx = 18m;
        public const int MaxIndicatorSnapshotAgeMinutes = 2880;
    }
}
