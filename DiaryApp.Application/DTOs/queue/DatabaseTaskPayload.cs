namespace DiaryApp.Application.DTOs.Queue;

public enum DbTaskType
{
    LinkMomentsToLog,
    // other: CalculateDailyStreak, CleanupOldData...
}

public class DatabaseTaskPayload
{
    public DbTaskType TaskType { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string DateStr { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
}