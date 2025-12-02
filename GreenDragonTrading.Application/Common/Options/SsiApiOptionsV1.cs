namespace GreenDragonTrading.Application.Common.Options
{
    public class SsiApiOptionsV1
    {
        public const string SectionName = "SsiApiV1";
        public string IBoardQuery { get; set; } = null!;
        public string IBoardApi { get; set; } = null!;
        public int TimeoutSeconds { get; set; }
    }
}
