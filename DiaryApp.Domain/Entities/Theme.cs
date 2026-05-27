using System;
using DiaryApp.Domain.Enums;

namespace DiaryApp.Domain.Entities;

public class Theme
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public int Price { get; set; } = 0;
    public required string ThumbnailUrl { get; set; }
    public required string BackgroundUrl { get; set; }
    public string BackgroundDarkColor { get; set; } = "0xFFF4F6F1";
    public string BackgroundLightColor { get; set; } = "0xFF1C1C1C";
    public string PrimaryDarkColor { get; set; } = "0xFFF4F6F1";
    public string PrimaryLightColor { get; set; } = "0xFF1C1C1C";
    public string AuthorId { get; set; } = string.Empty;
    public bool IsOfficial { get; set; } = false;
    public bool IsActive { get; set; } = true;

    public List<ThemeMoodIcon> Moods { get; set; } = new();
}

public class ThemeMoodIcon
{
    public BaseMood BaseMoodId { get; set; } // from 1 to 5
    public required string CustomName { get; set; } // name of icon from 1 to 5
    public required string IconColor { get; set; }
}
