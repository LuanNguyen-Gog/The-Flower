using System.ComponentModel.DataAnnotations;

namespace Service.DTOs.Users;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Avatar { get; set; }
    public string Role { get; set; } = "Customer";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UpdateProfileDto
{
    [Required]
    [MaxLength(50)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(15)]
    public string? PhoneNumber { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    [MaxLength(500)]
    public string? Avatar { get; set; }
}

public class ChangeUserPasswordDto
{
    [Required]
    public string OldPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}
