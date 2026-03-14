using System.ComponentModel.DataAnnotations;

namespace Service.DTOs.Auth;

public class ChangePasswordDto
{
    [Required(ErrorMessage = "Old password is required.")]
    public string OldPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm password is required.")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
