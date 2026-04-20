namespace DiaryApp.Application.DTOs.DailyLog;

public class DailyLogResponseDto
{
    public string Id { get; set; }  = "";
    public int? BaseMoodId { get; set; }
    public string Date { get; set; } = null!;
    public string? Note { get; set; }
    public double SleepHours { get; set; }
    public bool IsMenstruation { get; set; }
    public string? MenstruationPhase { get; set; }
    public List<string> DailyPhotos { get; set; } = new();
    public List<string> ActivityIds { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

