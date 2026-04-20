namespace DiaryApp.Application.DTOs.Activity;

public class ActivityResponseDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string IconUrl { get; set; }
    public required string Category { get; set; }
}