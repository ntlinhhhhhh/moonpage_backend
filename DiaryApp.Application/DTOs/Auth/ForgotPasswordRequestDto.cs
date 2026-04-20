using System.ComponentModel.DataAnnotations;

namespace DiaryApp.Application.DTOs.Auth;

public class ForgotPasswordRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public required string Email { get; set; }
}