using System.Runtime.CompilerServices;

namespace DiaryApp.Application.DTOs.Theme;

public class ThemeMoodResponseDto
{
    public string BaseMoodId { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public string? CustomName { get; set; }
}
