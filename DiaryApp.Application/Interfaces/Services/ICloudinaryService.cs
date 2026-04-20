using System.IO;

namespace DiaryApp.Application.Interfaces.Services;

public interface ICloudinaryService
{
    Task<string?> UploadImageAsync(Stream fileStream, string fileName, string folderName);
    
    Task<string?> UploadImageFromUrlAsync(string imageUrl, string folderName);
    
    Task<bool> DeleteImageAsync(string publicId);
}