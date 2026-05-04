namespace GreenDragonTrading.Application.Common.Options
{
    public class MomoOptions
    {
        public const string SectionName = "Momo";

        public string ApiEndpoint { get; set; } = "https://test-payment.momo.vn/gw_payment/transactionProcessor";

        /// <summary>
        /// V2 API endpoint for querying transaction status.
        /// </summary>
        public string QueryEndpoint { get; set; } = "https://test-payment.momo.vn/v2/gateway/api/query";

        public string SecretKey { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string PartnerCode { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string NotifyUrl { get; set; } = string.Empty;
        public string RequestType { get; set; } = "captureMoMoWallet";

        /// <summary>
        /// Minutes after which a Pending Momo order is considered expired.
        /// Momo sandbox default is 15 minutes; production may differ.
        /// </summary>
        public int ExpirationMinutes { get; set; } = 15;
    }
}
