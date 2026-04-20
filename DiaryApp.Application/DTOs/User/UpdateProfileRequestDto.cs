using System.ComponentModel.DataAnnotations;

namespace DiaryApp.Application.DTOs.User;

public class UpdateProfileRequestDto
{
    [Required(ErrorMessage = "Name is required.")]
    public required string Name { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Gender { get; set; }
    public string? Birthday { get; set; }
}