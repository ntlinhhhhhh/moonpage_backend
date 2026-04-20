using System.ComponentModel.DataAnnotations;

namespace DiaryApp.Application.DTOs.Auth;

public class ResetPasswordRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "OTP is required.")]
    public required string ResetToken { get; set; }

    [Required(ErrorMessage = "New password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    public required string NewPassword { get; set; }
}