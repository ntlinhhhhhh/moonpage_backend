namespace DiaryApp.Domain.Entities;

public class AppNotification
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "System";
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; }
}