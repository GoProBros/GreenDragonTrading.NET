namespace GreenDragonTrading.Application.DTOs
{
    /// <summary>
    /// Information extracted from JWT token
    /// </summary>
    public class TokenInfo
    {
        public string Jti { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; } = null!;
    }
}
