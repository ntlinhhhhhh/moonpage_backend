namespace DiaryApp.Application.DTOs.Theme;

public class ThemeResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Price { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? BackgroundUrl { get; set; }

}
