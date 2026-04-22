using System;
using System.Collections.Generic;

namespace DiaryApp.Domain.Entities;

public class DailyLog
{
    public required string Id { get; set; } 
    public string UserId { get; set; } = null!;
    public int? BaseMoodId { get; set; }
    public double SleepHours { get; set; } = 0;
    public bool IsMenstruation { get; set; } = false;
    public string? MenstruationPhase { get; set; } // detail of menstruation
    public string? Note { get; set; }
    public string Date { get; set; } = string.Empty;
    public required string YearMonth { get; set; }

    // daily photos
    public List<string> DailyPhotos { get; set; } = new();

    // activities list
    public List<string> ActivityIds { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
