using System.ComponentModel.DataAnnotations;

namespace DiaryApp.Application.DTOs.Auth;

public class VerifyOtpRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "OTP code is required.")]
    public required string OtpCode { get; set; }
}