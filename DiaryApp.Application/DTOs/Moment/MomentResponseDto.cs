namespace DiaryApp.Application.DTOs.Moment;

public class MomentResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserAvatarUrl { get; set; } = string.Empty;
    public string DailyLogId { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CapturedAt { get; set; }
}