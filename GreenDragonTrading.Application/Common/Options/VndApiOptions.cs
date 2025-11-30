namespace GreenDragonTrading.Application.Common.Options
{
    public class VndApiOptions
    {
        public const string SectionName = "VndApi";
        public string BaseUrl { get; set; } = null!;
        public int TimeoutSeconds { get; set; }
    }
}
