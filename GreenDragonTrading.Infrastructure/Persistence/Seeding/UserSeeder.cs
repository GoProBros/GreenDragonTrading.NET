using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography; 
using System.Text;

namespace GreenDragonTrading.Infrastructure.Persistence.Seeding
{
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
            var adminId = new Guid("11111111-1111-1111-1111-111111111111");
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
                    CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    Status = CommonStatus.Active,
                    IsEmailVerified = true
                }
            );
        }
    }
}
