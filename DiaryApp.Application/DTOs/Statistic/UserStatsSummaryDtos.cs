namespace DiaryApp.Application.DTOs.Statistic;

public class UserStatsSummaryDto 
{
    public int TotalLogs { get; set; }
    public int TotalPhotos { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }

    // Mood Pie Chart
    public List<MoodDistributionDto> MoodDistribution { get; set; } = new();
    
    // Mood Line/Bar Chart
    public List<MoodFlowDto> MoodFlow { get; set; } = new();
    
    // Top activity
    public List<ActivityInfluenceDto> BestActivities { get; set; } = new();

    // New stats
    public int TotalSteps { get; set; }
    public double AverageSleepHours { get; set; }
    public string? AverageSleepStartTime { get; set; }
    public List<SleepDataDto> SleepAnalysis { get; set; } = new();
    public List<string> MusicSummary { get; set; } = new();
}

public class SleepDataDto
{
    public string Date { get; set; } = string.Empty;
    public string? StartTime { get; set; }
    public double Duration { get; set; }
    public int? MoodId { get; set; }
}