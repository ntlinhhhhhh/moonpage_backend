using System.ComponentModel.DataAnnotations;
using DiaryApp.Domain.Enums;

namespace DiaryApp.Application.DTOs.Theme;

public class CreateThemeMoodDto
{
    [Required]
    public BaseMood BaseMoodId { get; set; }

    [Required]
    public string IconUrl { get; set; } = string.Empty;

    public string? CustomName { get; set; }
}

public class CreateThemeRequestDto
{
    [Required]
    public string Id { get; set; } = string.Empty; // Admin define
    
    [Required(ErrorMessage = "Theme name is required.")]
    public string Name { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Please enter a valid price.")]
    public int Price { get; set; }

    public string? ThumbnailUrl { get; set; }
    public string? BackgroundUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public List<CreateThemeMoodDto> Moods { get; set; } = new();
}