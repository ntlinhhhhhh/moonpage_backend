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
}