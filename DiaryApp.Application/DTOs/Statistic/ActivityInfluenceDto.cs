namespace DiaryApp.Application.DTOs.Statistic;

public class ActivityInfluenceDto 
{
    public string ActivityId { get; set; } = null!;
    public string ActivityName { get; set; } = null!;
    public string IconUrl { get; set; } = null!;
    public double AverageMoodScore { get; set; }
    public int Occurrence { get; set; } // Số lần xuất hiện trong kỳ
}