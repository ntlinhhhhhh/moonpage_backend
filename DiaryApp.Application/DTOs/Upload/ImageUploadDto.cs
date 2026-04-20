public enum ImageUploadType
{
    Avatar,
    DailyLog,
    Moment
}

public class ImageUploadPayload
{
    public string UserId { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public ImageUploadType UploadType { get; set; }
    public string TempImagePath { get; set; } = string.Empty;
    
    public string? Caption { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CapturedAt { get; set; }
}
