using System;
using System.Collections.Generic;

namespace DiaryApp.Domain.Entities;

public class DailyLog
{
    public required string Id { get; set; } 
    public string UserId { get; set; } = null!;
    public int? BaseMoodId { get; set; }
    public double SleepHours { get; set; } = 0;
    public string? SleepStartTime { get; set; } // HH:mm
    public string? WakeupTime { get; set; } // HH:mm
    public bool IsMenstruation { get; set; } = false;
    public string? MenstruationPhase { get; set; } // detail of menstruation
    public int Steps { get; set; } = 0;
    public int Calories { get; set; } = 0;
    public double Distance { get; set; } = 0;
    public string? MusicTitle { get; set; }
    public string? ArtistName { get; set; }
    public string? AlbumArtUrl { get; set; }
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
