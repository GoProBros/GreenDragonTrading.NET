using GreenDragonTrading.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id", TypeName = "uuid")] 
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(24)]
        [Column("username", TypeName = "varchar(24)")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("email", TypeName = "varchar(255)")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Column("phone_number", TypeName = "varchar(20)")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("hashed_password", TypeName = "varchar(255)")]
        public string HashedPassword { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("avatar_url", TypeName = "varchar")] 
        public string? AvatarUrl { get; set; }

        [Required]
        [Column("role", TypeName = "smallint")]
        public UserRole Role { get; set; } = UserRole.User;

        [Required]
        [Column("created_at", TypeName = "timestamp with time zone")] 
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        [Column("status", TypeName = "smallint")]
        public CommonStatus Status { get; set; } = CommonStatus.Active;

        [Required]
        [Column("is_email_verified", TypeName = "boolean")]
        public bool IsEmailVerified { get; set; } = false;
    }
}
