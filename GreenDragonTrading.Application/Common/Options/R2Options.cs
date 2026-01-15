namespace GreenDragonTrading.Application.Common.Options
{
    /// <summary>
    /// Cloudflare R2 storage configuration options.
    /// </summary>
    public class R2Options
    {
        public const string SectionName = "FileStorage:R2";
        
        public string AccountId { get; set; } = null!;
        
        public string BucketName { get; set; } = null!;
        
        /// <summary>
        /// Auto-computed R2 endpoint URL.
        /// </summary>
        public string Endpoint => $"https://{AccountId}.r2.cloudflarestorage.com";
    }
}
