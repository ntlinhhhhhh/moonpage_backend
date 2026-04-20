using System;

namespace DiaryApp.Domain.Entities;

    public class Moment
{
    public string? Id { get; set; }
    public required string UserId { get; set; }
    public required string UserName { get; set; } 
    public required string UserAvatarUrl { get; set; }
    public required string DailyLogId { get; set; } // Date string "YYYY-MM-DD"
    public required string ImageUrl { get; set; }
    public string? Caption { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CapturedAt { get; set; }
}