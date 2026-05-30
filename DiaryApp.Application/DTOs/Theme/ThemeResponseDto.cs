namespace DiaryApp.Application.DTOs.Theme;

public class ThemeResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Price { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? BackgroundUrl { get; set; }
    public string BackgroundDarkColor { get; set; } = string.Empty;
    public string BackgroundLightColor { get; set; } = string.Empty;
    public string PrimaryDarkColor { get; set; } = string.Empty;
    public string PrimaryLightColor { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public bool IsOfficial { get; set; }
    public object? Description { get; set; }
    public List<ThemeMoodResponseDto> Moods { get; set; } = new();
}
