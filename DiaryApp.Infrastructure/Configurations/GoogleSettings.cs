namespace DiaryApp.Infrastructure.Configurations;

public class GoogleCloudSettings
{
    public string ProjectId { get; set; } = string.Empty;
    
    public string ClientId { get; set; } = string.Empty;
    
    public string StorageBucket { get; set; } = string.Empty;
    
    public string ServiceAccountPath { get; set; } = string.Empty;
}