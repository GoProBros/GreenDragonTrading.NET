using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography; 
using System.Text;

public static class UserSeeder
{
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
    }

    public static void Seed(ModelBuilder modelBuilder)
    {
        var adminId = Guid.NewGuid();
        var hashedPassword = HashPassword("123123");

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = adminId,
                Username = "admin",
                Email = "greendragon.trading.team@gmail.com",
                PhoneNumber = "0988671875",
                HashedPassword = hashedPassword,
                Role = UserRole.Admin,
                CreatedAt = DateTimeOffset.UtcNow,
                Status = CommonStatus.Active,
                IsEmailVerified = true 
            }
        );
    }
}