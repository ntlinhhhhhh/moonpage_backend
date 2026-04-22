namespace DiaryApp.Application.DTOs.Statistic;

public class MoodDistributionDto 
{
    public string Label { get; set; } = null!; // mood
    public int Count { get; set; }
    public double Percentage { get; set; } // pie chart
}