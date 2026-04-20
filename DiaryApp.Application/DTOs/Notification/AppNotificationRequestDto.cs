namespace DiaryApp.Application.DTOs;

public class AppNotificationRequestDto
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Type { get; set; }
}