using System.ComponentModel.DataAnnotations;

namespace DiaryApp.Application.DTOs.Activity;

public class ActivityRequestDto
{
    [Required(ErrorMessage = "Activity name is required.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Icon URL is required.")]
    public string IconUrl { get; set; } = null!;

    public string? Category { get; set; }
}