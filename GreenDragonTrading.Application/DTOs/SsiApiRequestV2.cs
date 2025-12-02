namespace GreenDragonTrading.Application.DTOs
{
    public class AccessTokenRequest
    {
        public string ConsumerID { get; set; } = null!;
        public string ConsumerSecret { get; set; } = null!;
    }

    public class SecuritiesDetailsRequest
    {
        public string? Market { get; set; }
        public string? Symbol { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 1000;
    }
}
