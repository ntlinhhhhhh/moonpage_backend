namespace DiaryApp.Domain.Entities;

public class Activity
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string IconUrl { get; set; }
    public string? Category { get; set; } // 'sport', 'work', 'relax'
}