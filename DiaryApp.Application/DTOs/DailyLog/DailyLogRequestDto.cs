using Microsoft.AspNetCore.Http;

namespace DiaryApp.Application.DTOs.DailyLog;

public class DailyLogRequestDto
{
    public int? BaseMoodId { get; set; }
    public string Date { get; set; } = null!;
    public string? Note { get; set; }
    public double SleepHours { get; set; }
    public bool IsMenstruation { get; set; }
    public string? MenstruationPhase { get; set; }
    public List<IFormFile> DailyPhotos { get; set; } = new();
    public List<string> ActivityIds { get; set; } = new();
}

