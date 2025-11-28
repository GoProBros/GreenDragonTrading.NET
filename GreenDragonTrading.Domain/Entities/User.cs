using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Domain.Entities;

[Table("users")]
public class User
{
    [Key]
    [Column("id")]
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
    [MaxLength(255)]
    [Column("hashed_password", TypeName = "varchar(255)")]
    public string HashedPassword { get; set; } = string.Empty;

    [Column("avatar_url", TypeName = "varchar")]
    public string? AvatarUrl { get; set; }

    [Required]
    [Column("role", TypeName = "smallint")]
    public UserRole Role { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
