namespace GreenDragonTrading.Application.Common.Options
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";
        public string Secret { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int AccessTokenExpirationMinutes { get; set; }
        public int RefreshTokenExpirationDays { get; set; }
    }
}
