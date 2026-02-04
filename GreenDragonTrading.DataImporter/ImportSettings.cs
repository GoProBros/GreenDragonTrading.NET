namespace GreenDragonTrading.DataImporter;

public class ImportSettings
{
    public int BatchSize { get; set; } = 10;
    public int DelayBetweenBatchesMs { get; set; } = 1000;
    public int MaxRetries { get; set; } = 3;
    public int RetryDelayMs { get; set; } = 2000;
}
