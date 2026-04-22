using DiaryApp.Domain.Enums;

namespace DiaryApp.Domain.Entities;

public class User
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string NameLower { get; set; } = "";
    public required string Email { get; set; }
    public UserRole Role { get; set; } = 0;
    public required string HashPassword {get; set; }
    public string? Gender { get; set; }
    public string? Birthday { get; set; }
    public string AvatarUrl { get; set; } = "avatar_default.png";
    public int CoinBalance { get; set; }
    public string ActiveThemeId { get; set; } = "theme_default_id";
    public List<string> OwnedThemeIds { get; set; } = new List<string>(); // Collection theme
    public string AuthProvider { get; set; } = "Local";
    public string? ResetOtp { get; set; }
    public DateTime? OtpExpiry { get; set; }
    // public int CurrentStreak { get; set; } = 0;
    // public DateTime? LastRewardDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}