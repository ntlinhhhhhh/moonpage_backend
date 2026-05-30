using System.Runtime.CompilerServices;

namespace DiaryApp.Application.DTOs.Theme;

public class ThemeMoodResponseDto
{
    public int BaseMoodId { get; set; }
    public string IconColor { get; set; } = string.Empty;
    public string? CustomName { get; set; }
}
