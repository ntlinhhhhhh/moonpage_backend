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
    public bool IsActive { get; set; } = true;

    // List 5 icon of theme
    public List<ThemeMoodIcon> Moods { get; set; } = new();
}

public class ThemeMoodIcon
{
    public BaseMood BaseMoodId { get; set; } // from 1 to 5
    public required string CustomName { get; set; } // name of icon from 1 to 5
    public required string IconUrl { get; set; }
}
