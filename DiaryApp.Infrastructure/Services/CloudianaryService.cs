using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DiaryApp.Application.Interfaces.Services;
using DiaryApp.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace DiaryApp.Infrastructure.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IOptions<CloudinarySettings> config)
    {
        var account = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret
        );

        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<string?> UploadImageAsync(Stream fileStream, string fileName, string folderName)
    {
        if (fileStream == null || fileStream.Length == 0) return null;

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = $"diary_app/{folderName}",
            Transformation = new Transformation().Quality("auto").FetchFormat("auto")
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
        
        if (uploadResult.Error != null)
        {
            throw new Exception($"Lỗi upload ảnh lên Cloudinary: {uploadResult.Error.Message}");
        }

        return uploadResult.SecureUrl?.ToString();
    }

    public async Task<string?> UploadImageFromUrlAsync(string imageUrl, string folderName)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(imageUrl),
            Folder = $"diary_app/{folderName}",
            Transformation = new Transformation().Quality("auto").FetchFormat("auto")
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
        return uploadResult.SecureUrl?.ToString();
    }

    public async Task<bool> DeleteImageAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deleteParams);
        return result.Result == "ok";
    }
}