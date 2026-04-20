using System.ComponentModel.DataAnnotations;

namespace DiaryApp.Application.DTOs.Auth;

public class RegisterRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    public required string Password { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(50, MinimumLength = 4, ErrorMessage = "Name must be between 4 and 50 characters.")]
    public required string Name { get; set; }
}