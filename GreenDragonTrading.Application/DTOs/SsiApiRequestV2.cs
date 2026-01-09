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

    public class IntradayOhlcRequest
    {
        public string Symbol { get; set; } = null!;
        public string FromDate { get; set; } = null!;
        public string ToDate { get; set; } = null!;
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public bool? Ascending { get; set; }
        public int? Resollution { get; set; }
    }
}
