namespace DiaryApp.Application.DTOs.User;

public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Gender { get; set; }
    public string? Birthday { get; set; }
    public int CoinBalance { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public int StreakFreezes { get; set; }
    public int RecoverableStreak { get; set; }
    public string ActiveThemeId { get; set; } = string.Empty;
    public string AuthProvider { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}