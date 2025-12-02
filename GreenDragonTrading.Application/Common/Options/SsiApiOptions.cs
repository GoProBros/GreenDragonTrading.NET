namespace GreenDragonTrading.Application.Common.Options
{
    public class SsiApiOptions
    {
        public const string SectionName = "SsiApi";
        public string IBoardQuery { get; set; } = null!;
        public string IBoardApi { get; set; } = null!;
        public int TimeoutSeconds { get; set; }
    }
}
