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

        string objectKey = $"diary_app/{folderName}/{fileName}";

        await storageClient.UploadObjectAsync(
            bucket: _bucketName,
            objectName: objectKey,
            contentType: "image/jpeg",
            source: fileStream
        );

        return objectKey;
    }

    public async Task<string?> UploadImageFromUrlAsync(string imageUrl, string folderName)
    {
        var response = await _httpClient.GetAsync(imageUrl);
        if (!response.IsSuccessStatusCode) return null;

        using var stream = await response.Content.ReadAsStreamAsync();
        string fileName = $"{Guid.NewGuid()}.jpg";
        
        return await UploadImageAsync(stream, fileName, folderName);
    }

    public string GetImageUrl(string objectKey)
    {
        if (string.IsNullOrEmpty(objectKey)) return string.Empty;

        if (objectKey.StartsWith("http://") || objectKey.StartsWith("https://")) 
        {
            return objectKey;
        }

        return $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o/{Uri.EscapeDataString(objectKey)}?alt=media";
    }

    public async Task<bool> DeleteImageAsync(string objectKeyOrUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(objectKeyOrUrl)) return false;

            var storageClient = await StorageClient.CreateAsync();
            string finalObjectKey = objectKeyOrUrl;
            
            if (objectKeyOrUrl.StartsWith("http"))
            {
                string standardPrefix = $"https://storage.googleapis.com/{_bucketName}/";
                
                if (objectKeyOrUrl.StartsWith(standardPrefix))
                {
                    finalObjectKey = objectKeyOrUrl.Replace(standardPrefix, "");
                }
                else if (objectKeyOrUrl.Contains("/o/"))
                {
                    var splitPath = objectKeyOrUrl.Split("/o/")[1].Split("?")[0];
                    finalObjectKey = Uri.UnescapeDataString(splitPath);
                }
            }

            await storageClient.DeleteObjectAsync(_bucketName, finalObjectKey);
            return true;
        }
        catch 
        { 
            return false; 
        }
    }
}