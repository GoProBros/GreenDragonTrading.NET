namespace GreenDragonTrading.Application.Common.Options
{
    public class SsiApiOptionsV2
    {
        public const string SectionName = "SsiApiV2";
        public string FastConnectUrl { get; set; } = null!;
        public string StreamURL { get; set; } = null!;
        public string ConsumerID { get; set; } = null!;
        public string ConsumerSecret { get; set; } = null!;
        public string PublicKey { get; set; } = null!;
        public string PrivateKey { get; set; } = null!;
        public int TimeoutSeconds { get; set; }
    }
}
