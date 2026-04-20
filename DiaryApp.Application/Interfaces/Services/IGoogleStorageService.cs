namespace DiaryApp.Application.Interfaces.Services;

public interface IGoogleStorageService
{
    Task<string?> UploadImageAsync(Stream fileStream, string fileName, string folderName);
    
    Task<string?> UploadImageFromUrlAsync(string imageUrl, string folderName);
    
    Task<bool> DeleteImageAsync(string fileUrl);
}