using Google.Cloud.Storage.V1;
using DiaryApp.Application.Interfaces.Services;
using DiaryApp.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace DiaryApp.Infrastructure.Services;

public class GoogleStorageService : IGoogleStorageService
{
    private readonly string _bucketName;
    private readonly HttpClient _httpClient;

    public GoogleStorageService(IOptions<GoogleCloudSettings> config, HttpClient httpClient)
    {
        _bucketName = config.Value.StorageBucket;
        _httpClient = httpClient;
    }

    public async Task<string?> UploadImageAsync(Stream fileStream, string fileName, string folderName)
    {
        if (fileStream == null || fileStream.Length == 0) return null;

        var storageClient = await StorageClient.CreateAsync();

        // url: diary_app/moments/image_name.jpg
        string objectName = $"diary_app/{folderName}/{fileName}";

        await storageClient.UploadObjectAsync(
            bucket: _bucketName,
            objectName: objectName,
            contentType: "image/jpeg",
            source: fileStream
        );

        return $"https://storage.googleapis.com/{_bucketName}/{objectName}";
    }

    public async Task<string?> UploadImageFromUrlAsync(string imageUrl, string folderName)
    {
        var response = await _httpClient.GetAsync(imageUrl);
        if (!response.IsSuccessStatusCode) return null;

        using var stream = await response.Content.ReadAsStreamAsync();
        string fileName = $"{Guid.NewGuid()}.jpg";
        
        return await UploadImageAsync(stream, fileName, folderName);
    }

    public async Task<bool> DeleteImageAsync(string fileUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(fileUrl)) return false;

            var storageClient = await StorageClient.CreateAsync();
            string prefix = $"https://storage.googleapis.com/{_bucketName}/";
            
            if (fileUrl.StartsWith(prefix))
            {
                string objectName = fileUrl.Replace(prefix, "");
                await storageClient.DeleteObjectAsync(_bucketName, objectName);
                return true;
            }
            return false;
        }
        catch { return false; }
    }
}