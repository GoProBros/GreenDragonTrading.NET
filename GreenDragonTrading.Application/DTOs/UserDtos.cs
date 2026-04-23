namespace GreenDragonTrading.Application.DTOs
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public UserDto User { get; set; } = null!;
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = null!;
        public bool IsEmailVerified { get; set; }
        public string? SubscriptionLevel { get; set; }
        public List<string> AllowedModules { get; set; } = new();
        public string? TelegramChatId { get; set; }
        public bool IsTelegramLinked { get; set; }
    }
}
