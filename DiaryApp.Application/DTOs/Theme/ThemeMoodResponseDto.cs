using System.Runtime.CompilerServices;

namespace DiaryApp.Application.DTOs.Theme;

public class ThemeMoodResponseDto
{
    public string BaseMoodId { get; set; } = string.Empty;
    public string IconColor { get; set; } = string.Empty;
    public string? CustomName { get; set; }
}
