using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DiaryApp.Domain.Enums;

namespace DiaryApp.Application.DTOs.Theme;

public class UploadThemeMoodDto
{
    [Required]
    public BaseMood BaseMoodId { get; set; }

    [Required]
    public string IconColor { get; set; } = string.Empty;

    public string? CustomName { get; set; }
}

public class UploadThemeRequestDto
{
    [Required]
    public string Id { get; set; } = string.Empty; 
    
    [Required(ErrorMessage = "Theme name is required.")]
    public string Name { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Please enter a valid price.")]
    public int Price { get; set; }

    public IFormFile? Thumbnail { get; set; }
    
    public IFormFile? Background { get; set; }

    public string? BackgroundDarkColor { get; set; }
    public string? BackgroundLightColor { get; set; }
    public string? PrimaryDarkColor { get; set; }
    public string? PrimaryLightColor { get; set; }

    public bool IsOfficial { get; set; } = false;
    public bool IsActive { get; set; } = true;

    public string? Description { get; set; }
    public string? Moods { get; set; } 
}
