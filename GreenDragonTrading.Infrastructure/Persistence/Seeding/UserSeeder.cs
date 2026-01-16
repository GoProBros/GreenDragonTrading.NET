using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net; // Đảm bảo đã cài package BCrypt.Net-Next

namespace GreenDragonTrading.Infrastructure.Persistence.Seeding
{
    public static class UserSeeder
    {
        public static readonly Guid AdminId = new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef");

        public static void Seed(ModelBuilder modelBuilder)
        {
            string adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL")
                                ?? "greendragon.trading.team@gmail.com";
            string adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
                                   ?? "Admin123";
            string phoneNumber = Environment.GetEnvironmentVariable("ADMIN_PHONE_NUMBER")
                                 ?? "0988671875";
            string name = Environment.GetEnvironmentVariable("Name")
                                 ?? "Admin";   
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(adminPassword);

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = AdminId,
                    Username = name,
                    Email = adminEmail,
                    PhoneNumber = phoneNumber,
                    HashedPassword = hashedPassword,
                    Role = UserRole.Admin,
                    CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    Status = CommonStatus.Active,
                    IsEmailVerified = true
                }
            );
        }
    }
}
