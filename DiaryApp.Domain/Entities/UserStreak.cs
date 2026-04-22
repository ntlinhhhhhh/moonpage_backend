namespace DiaryApp.Domain.Entities;

public class UserStreak
{
    public string UserId { get; set; } = null!;
    public int CurrentStreak { get; set; } = 0;
    public int LongestStreak { get; set; } = 0;
    public DateTime? LastLogDate { get; set; }
    public int StreakFreezes { get; set; } = 0; // có Freeze // ko reset về 0
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}