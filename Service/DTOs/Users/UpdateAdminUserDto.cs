using System.ComponentModel.DataAnnotations;

namespace Service.DTOs.Users;

public class UpdateAdminUserDto
{
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(15)]
    public string? PhoneNumber { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = "Customer";

    [MinLength(6)]
    public string? Password { get; set; }
}
