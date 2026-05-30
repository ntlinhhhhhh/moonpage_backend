using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using DiaryApp.Domain.Enums;

namespace DiaryApp.Application.DTOs.Theme;

public class CreateThemeMoodDto
{
    [Required]
    public BaseMood BaseMoodId { get; set; }

    [Required]
    public string IconColor { get; set; } = string.Empty;

    public string? CustomName { get; set; }
}

public class CreateThemeRequestDto
{
    [Required]
    public string Id { get; set; } = string.Empty; 
    
    [Required(ErrorMessage = "Theme name is required.")]
    public string Name { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Please enter a valid price.")]
    public int Price { get; set; }

    public string? ThumbnailUrl { get; set; }
    public string? BackgroundUrl { get; set; }
    public string BackgroundDarkColor { get; set; } = "0xFFF4F6F1";
    public string BackgroundLightColor { get; set; } = "0xFF1C1C1C";
    public string PrimaryDarkColor { get; set; } = "0xFFF4F6F1";
    public string PrimaryLightColor { get; set; } = "0xFF1C1C1C";

    public bool IsOfficial { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public object? Description { get; set; }

    public List<CreateThemeMoodDto> Moods { get; set; } = new();
}
